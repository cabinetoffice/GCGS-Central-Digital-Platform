import json
import os
import time
from datetime import datetime
from zoneinfo import ZoneInfo
import urllib.parse
import urllib.request
import logging
import html

import boto3
from botocore.exceptions import ClientError

logger = logging.getLogger()
logger.setLevel(logging.INFO)

_secrets = None
_versions_cache = None

STAGE_ORDER = ["orchestrator", "development", "staging", "integration", "production"]
STAGE_LABELS = {
    "orchestrator": "Orchestrator",
    "development": "Development",
    "staging": "Staging",
    "integration": "Integration",
    "production": "Production",
}
STAGE_SHOW_VERSIONS = {"development", "staging", "integration", "production"}

DEFAULT_SIRSI_PARAM = "cdp-sirsi-envs-service-version"
DEFAULT_FTS_PARAM = "cdp-sirsi-fts-envs-service-version"
DEFAULT_CFS_PARAM = "cdp-sirsi-cfs-envs-service-version"
UK_TZ = ZoneInfo("Europe/London")

STATE_EMOJI = {
    "SUCCEEDED": "✅",
    "FAILED": "🔴",
    "STOPPED": "🛑",
    "CANCELED": "⛔",
    "CANCELLED": "⛔",
    "SKIPPED": "🦘",
    "IN_PROGRESS": "🔄",
    "STARTED": "🔄",
    "PENDING": "⏳",
    "UNKNOWN": "⁉️",
}

LEGEND_ORDER = [
    ("IN_PROGRESS", "started/in progress"),
    ("SUCCEEDED", "succeeded"),
    ("FAILED", "failed"),
    ("STOPPED", "stopped"),
    ("CANCELED", "canceled"),
    ("SKIPPED", "skipped"),
    ("PENDING", "pending"),
    ("UNKNOWN", "unknown"),
]


def _get_secret():
    global _secrets
    if _secrets is not None:
        return _secrets
    secret_arn = os.environ["TEAMS_SECRET_ARN"]
    client = boto3.client("secretsmanager")
    resp = client.get_secret_value(SecretId=secret_arn)
    secret_str = resp.get("SecretString", "{}")
    _secrets = json.loads(secret_str)
    return _secrets


def _get_versions():
    global _versions_cache
    if _versions_cache is not None:
        return _versions_cache

    ssm = boto3.client("ssm")
    sirsi_param = os.environ.get("SIRSI_VERSIONS_PARAM", DEFAULT_SIRSI_PARAM)
    fts_param = os.environ.get("FTS_VERSIONS_PARAM", DEFAULT_FTS_PARAM)
    cfs_param = os.environ.get("CFS_VERSIONS_PARAM", DEFAULT_CFS_PARAM)

    names = [sirsi_param, fts_param, cfs_param]
    resp = ssm.get_parameters(Names=names, WithDecryption=True)
    values = {p["Name"]: p["Value"] for p in resp.get("Parameters", [])}

    def _parse(name):
        raw = values.get(name, "{}")
        try:
            return json.loads(raw)
        except json.JSONDecodeError:
            logger.warning("Failed to parse SSM param %s", name)
            return {}

    sirsi = _parse(sirsi_param)
    fts = _parse(fts_param)
    cfs = _parse(cfs_param)

    versions = {}
    for env in STAGE_ORDER:
        versions[env] = {
            "S": sirsi.get(env, "n/a"),
            "F": fts.get(env, "n/a"),
            "C": cfs.get(env, "n/a"),
        }

    _versions_cache = versions
    return versions


def _token_request(secret):
    tenant = secret["TENANT_ID"]
    url = f"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token"
    data = {
        "client_id": secret["CLIENT_ID"],
        "client_secret": secret["CLIENT_SECRET"],
        "username": secret["USERNAME"],
        "password": secret["PASSWORD"],
        "grant_type": "password",
        "scope": "https://graph.microsoft.com/.default",
    }
    body = urllib.parse.urlencode(data).encode("utf-8")
    req = urllib.request.Request(url, data=body, method="POST")
    req.add_header("Content-Type", "application/x-www-form-urlencoded")
    try:
        with urllib.request.urlopen(req, timeout=10) as resp:
            payload = json.loads(resp.read().decode("utf-8"))
        return payload["access_token"]
    except urllib.error.HTTPError as e:
        body = e.read().decode("utf-8") if e.fp else ""
        logger.error("Token request failed %s: %s", e.code, body)
        raise


def _graph_request(method, url, token, payload):
    data = json.dumps(payload).encode("utf-8")
    req = urllib.request.Request(url, data=data, method=method)
    req.add_header("Authorization", f"Bearer {token}")
    req.add_header("Content-Type", "application/json")
    try:
        with urllib.request.urlopen(req, timeout=10) as resp:
            body = resp.read().decode("utf-8")
            return resp.status, json.loads(body) if body else {}
    except urllib.error.HTTPError as e:
        body = e.read().decode("utf-8") if e.fp else ""
        logger.error("Graph error %s: %s", e.code, body)
        return e.code, json.loads(body) if body else {"error": body}


def _pick_state(detail):
    return detail.get("state") or detail.get("build-status") or "UNKNOWN"


def _emoji_for_stage(state):
    return STATE_EMOJI.get((state or "").upper(), "⚪")


def _parse_iso(value):
    if not value:
        return None
    try:
        if isinstance(value, (int, float)):
            return int(value)
        text = str(value).replace("Z", "+00:00")
        return int(datetime.fromisoformat(text).timestamp())
    except (TypeError, ValueError):
        return None


def _format_time(epoch):
    if not epoch:
        return ""
    return datetime.fromtimestamp(int(epoch), tz=UK_TZ).strftime("%H:%M:%S")


def _format_duration(start_epoch, end_epoch):
    if not start_epoch or not end_epoch:
        return ""
    seconds = max(0, int(end_epoch) - int(start_epoch))
    hours, rem = divmod(seconds, 3600)
    minutes, secs = divmod(rem, 60)
    if hours:
        return f"{hours}:{minutes:02d}:{secs:02d}"
    return f"{minutes}:{secs:02d}"


def _execution_key(event, detail):
    pipeline = detail.get("pipeline")
    execution_id = detail.get("execution-id") or detail.get("executionId") or detail.get("execution_id")
    if pipeline and execution_id:
        return f"{pipeline}:{execution_id}"
    if execution_id:
        return str(execution_id)
    for key in ("build-id", "buildId", "deploymentId", "action-execution-id", "taskArn"):
        if key in detail:
            return str(detail[key])
    if "id" in event:
        return str(event["id"])
    return f"event-{int(time.time())}"


def _stage_from_event(detail):
    stage = str(detail.get("stage", "")).lower()
    action = str(detail.get("action", "")).lower()
    combined = f"{stage} {action}"
    for key in STAGE_ORDER:
        if key in combined:
            return key
    return None


def _init_stages():
    return {key: {"state": "PENDING", "start_epoch": None, "end_epoch": None} for key in STAGE_ORDER}


def _build_stage(existing, state, event_epoch):
    state = (state or "PENDING").upper()
    stage = dict(existing or {})
    start_epoch = stage.get("start_epoch")
    end_epoch = stage.get("end_epoch")

    if state in ("STARTED", "IN_PROGRESS", "PENDING"):
        if not start_epoch and event_epoch:
            start_epoch = event_epoch
    else:
        if not start_epoch and event_epoch:
            start_epoch = event_epoch
        if event_epoch:
            end_epoch = event_epoch

    stage.update(
        {
            "state": state,
            "start_epoch": start_epoch,
            "end_epoch": end_epoch,
        }
    )
    return stage


def _build_html_summary(item, versions, execution_id, link, commit_url, commit_id):
    commit_message = item.get("commit_message")
    header_lines = []
    if commit_url and commit_id:
        short_commit = str(commit_id)[:7]
        header_lines.append(f'Deployment revision: <a href="{commit_url}">{html.escape(short_commit)}</a></br>')
    elif commit_url:
        header_lines.append(f'Deployment revision: <a href="{commit_url}">Open commit</a></br>')

    rows = []

    for key in STAGE_ORDER:
        label = STAGE_LABELS[key]
        stage = item["stages"].get(key, {"state": "PENDING", "start_epoch": None, "end_epoch": None})
        emoji = _emoji_for_stage(stage.get("state"))
        start_time = _format_time(stage.get("start_epoch")) or "-"
        duration = _format_duration(stage.get("start_epoch"), stage.get("end_epoch")) or "-"
        if key in STAGE_SHOW_VERSIONS:
            v = versions.get(key, {})
            s_val = v.get("S", "n/a")
            f_val = v.get("F", "n/a")
            c_val = v.get("C", "n/a")
        else:
            s_val = f_val = c_val = "-"

        rows.append(
            "<tr>"
            f"<td>{emoji}</td>"
            f"<td>{html.escape(label)}</td>"
            f"<td>{html.escape(start_time)}</td>"
            f"<td>{html.escape(str(s_val))}</td>"
            f"<td>{html.escape(str(f_val))}</td>"
            f"<td>{html.escape(str(c_val))}</td>"
            f"<td>{html.escape(duration)}</td>"
            "</tr>"
        )

    table_html = (
        "<table>"
        "<thead><tr><th></th><th>Env</th><th>Start</th><th>SIRSI</th><th>FTS</th><th>CFS</th><th>Took</th></tr></thead>"
        "<tbody>"
        + "".join(rows)
        + "</tbody></table>"
    )

    footer_lines = []
    if commit_message:
        commit_html = html.escape(str(commit_message)).replace("\n", "<br/>")
        footer_lines.append("<b>GCGS-Central-Digital-Platform Commit:</b>")
        footer_lines.append(commit_html)
    if execution_id and link:
        footer_lines.append(f'<b>Execution:</b><br> <a href="{link}">{html.escape(str(execution_id))}</a>')
    elif execution_id:
        footer_lines.append(f"Execution: {html.escape(str(execution_id))}")
    parts = []
    if header_lines:
        parts.append("<br/>".join(header_lines))
    parts.append(table_html)
    if footer_lines:
        spacer = "<br/><br/><br/>"
        parts.append(spacer + "<br/>".join(footer_lines))

    legend = ["<hr/>", "Legend:"]
    for key, label in LEGEND_ORDER:
        legend.append(f"{STATE_EMOJI.get(key, '⁉️')} {label}")
    parts.append("<br/>".join(legend))

    return "<br/>".join(parts)


def _build_message_payload(html_content):
    return {
        "body": {"contentType": "html", "content": html_content},
    }


def _build_reply_payload(handler_mention):
    secret = _get_secret()
    handler_id = secret.get("HANDLER_AAD_ID")
    handler_name = secret.get("HANDLER_DISPLAY_NAME", handler_mention)
    if not handler_id:
        return None
    content = f'<at id="0">{html.escape(handler_name)}</at> stage failed. Please check.'
    payload = {
        "body": {"contentType": "html", "content": content},
        "mentions": [
            {
                "id": 0,
                "mentionText": handler_name,
                "mentioned": {
                    "user": {
                        "id": handler_id,
                        "displayName": handler_name,
                    }
                },
            }
        ],
    }
    return payload


def _github_commit_url(external_url):
    if not external_url:
        return None, None
    try:
        parsed = urllib.parse.urlparse(external_url)
        params = urllib.parse.parse_qs(parsed.query)
        repo = params.get("FullRepositoryId", [None])[0]
        commit = params.get("Commit", [None])[0]
        if repo and commit:
            return f"https://github.com/{repo}/commit/{commit}", commit
    except Exception:
        return None, None
    return None, None


def handler(event, _context):
    logger.info("Event: %s", json.dumps(event))
    secret = _get_secret()
    token = _token_request(secret)

    team_id = secret["TEAM_ID"]
    channel_id = secret["CHANNEL_ID"]
    graph_base = os.environ.get("GRAPH_BASE", "https://graph.microsoft.com/v1.0")

    detail = event.get("detail", {})
    execution_key = _execution_key(event, detail)
    versions = _get_versions()

    pipeline = detail.get("pipeline") or detail.get("project-name")
    execution_id = detail.get("execution-id") or detail.get("executionId") or detail.get("execution_id")
    link = None
    if pipeline and execution_id:
        link = (
            "https://eu-west-2.console.aws.amazon.com/codesuite/codepipeline/pipelines/"
            f"{urllib.parse.quote(str(pipeline))}/executions/{urllib.parse.quote(str(execution_id))}/visualization?region=eu-west-2"
        )

    table = boto3.resource("dynamodb").Table(os.environ["TEAMS_MESSAGE_TABLE"])
    existing = table.get_item(Key={"execution_key": execution_key}).get("Item")
    item = existing or {"execution_key": execution_key}
    if "stages" not in item:
        item["stages"] = _init_stages()
    if pipeline:
        item["pipeline"] = pipeline
    if execution_id:
        item["execution_id"] = execution_id

    commit_message = detail.get("commit-message") or detail.get("commitMessage") or detail.get("commit_message")
    commit_url = None
    commit_id = None
    if not commit_message:
        execution_result = detail.get("execution-result", {})
        summary = execution_result.get("external-execution-summary")
        if summary:
            try:
                summary_obj = json.loads(summary)
                commit_message = summary_obj.get("CommitMessage")
            except json.JSONDecodeError:
                commit_message = None
    execution_result = detail.get("execution-result", {})
    commit_url = execution_result.get("external-execution-url")
    commit_id = execution_result.get("external-execution-id")
    github_url, github_commit = _github_commit_url(commit_url)
    if github_url:
        commit_url = github_url
        commit_id = github_commit or commit_id
    if commit_message:
        item["commit_message"] = commit_message

    stage_key = _stage_from_event(detail)
    state = _pick_state(detail)
    event_epoch = _parse_iso(event.get("time")) or _parse_iso(detail.get("start-time"))

    if not stage_key and state in ("STARTED", "IN_PROGRESS"):
        stage_key = "orchestrator"

    stage_updates = None
    if stage_key:
        existing_stage = item["stages"].get(stage_key) if item.get("stages") else None
        stage_updates = _build_stage(existing_stage, state, event_epoch)

    orchestrator_updates = None
    if stage_key != "orchestrator":
        orchestrator_stage = item.get("stages", {}).get("orchestrator", {})
        if orchestrator_stage.get("state") == "PENDING" and event_epoch:
            orchestrator_updates = _build_stage(orchestrator_stage, "STARTED", event_epoch)

    # Ensure stages exists without overlapping update paths.
    try:
        table.update_item(
            Key={"execution_key": execution_key},
            UpdateExpression="SET #stages = if_not_exists(#stages, :empty_stages)",
            ExpressionAttributeNames={"#stages": "stages"},
            ExpressionAttributeValues={":empty_stages": _init_stages()},
            ConditionExpression="attribute_not_exists(#stages)",
        )
    except ClientError as exc:
        if exc.response.get("Error", {}).get("Code") != "ConditionalCheckFailedException":
            raise

    update_exprs = []
    expr_names = {}
    expr_values = {}

    if stage_key and stage_updates:
        update_exprs.append("#stages.#stage = :stage")
        expr_names["#stages"] = "stages"
        expr_names["#stage"] = stage_key
        expr_values[":stage"] = stage_updates
    if orchestrator_updates:
        update_exprs.append("#stages.#orchestrator = :orchestrator")
        expr_names["#stages"] = "stages"
        expr_names["#orchestrator"] = "orchestrator"
        expr_values[":orchestrator"] = orchestrator_updates

    if pipeline:
        update_exprs.append("pipeline = :pipeline")
        expr_values[":pipeline"] = pipeline
    if execution_id:
        update_exprs.append("execution_id = :execution_id")
        expr_values[":execution_id"] = execution_id
    if commit_message:
        update_exprs.append("commit_message = :commit_message")
        expr_values[":commit_message"] = commit_message
    if commit_url:
        update_exprs.append("commit_url = if_not_exists(commit_url, :commit_url)")
        expr_values[":commit_url"] = commit_url
    if commit_id:
        update_exprs.append("commit_id = if_not_exists(commit_id, :commit_id)")
        expr_values[":commit_id"] = commit_id

    update_exprs.append("updated_at = :updated_at")
    expr_values[":updated_at"] = int(time.time())

    update_kwargs = {
        "Key": {"execution_key": execution_key},
        "UpdateExpression": "SET " + ", ".join(update_exprs),
        "ExpressionAttributeValues": expr_values,
        "ReturnValues": "ALL_NEW",
    }
    if expr_names:
        update_kwargs["ExpressionAttributeNames"] = expr_names
    resp = table.update_item(**update_kwargs)
    item = resp.get("Attributes", item)

    handler_mention = None
    failed_states = {"FAILED", "STOPPED", "CANCELED", "CANCELLED"}
    if stage_key and state in failed_states:
        handler_mention = secret.get("HANDLER_DISPLAY_NAME") or "handler"

    commit_url = item.get("commit_url") or commit_url
    commit_id = item.get("commit_id") or commit_id
    html_content = _build_html_summary(item, versions, execution_id, link, commit_url, commit_id)
    message_id = item.get("message_id")

    if message_id:
        url = f"{graph_base}/teams/{team_id}/channels/{channel_id}/messages/{message_id}"
        status, body = _graph_request("PATCH", url, token, _build_message_payload(html_content))
        if status == 404:
            message_id = None
        elif status >= 400:
            return {"status": status, "error": body, "execution_key": execution_key}

    if not message_id:
        url = f"{graph_base}/teams/{team_id}/channels/{channel_id}/messages"
        status, body = _graph_request("POST", url, token, _build_message_payload(html_content))
        if status >= 400:
            return {"status": status, "error": body, "execution_key": execution_key}
        message_id = body.get("id")

    if message_id:
        table.update_item(
            Key={"execution_key": execution_key},
            UpdateExpression="SET message_id = :message_id, updated_at = :updated_at",
            ExpressionAttributeValues={
                ":message_id": message_id,
                ":updated_at": int(time.time()),
            },
        )

    if handler_mention and message_id:
        reply_payload = _build_reply_payload(handler_mention)
        if reply_payload:
            reply_url = f"{graph_base}/teams/{team_id}/channels/{channel_id}/messages/{message_id}/replies"
            _graph_request("POST", reply_url, token, reply_payload)

    return {
        "status": 200,
        "execution_key": execution_key,
        "message_id": message_id,
    }
