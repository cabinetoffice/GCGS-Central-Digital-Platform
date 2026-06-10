import json
import os
import time
import urllib.parse
import urllib.request
import logging
import html

import boto3

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
    state = (state or "").upper()
    if state in ("FAILED", "STOPPED"):
        return "❌"
    if state in ("SUCCEEDED",):
        return "✅"
    if state in ("STARTED", "IN_PROGRESS", "PENDING"):
        return "⏳"
    return "🟠"


def _execution_key(event, detail):
    for key in ("execution-id", "executionId", "execution_id", "build-id", "buildId", "deploymentId", "action-execution-id", "taskArn"):
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
    return {key: {"state": "PENDING", "ts": ""} for key in STAGE_ORDER}


def _format_summary(item, versions, pipeline, execution_id, link):
    commit_message = item.get("commit_message") or "(commit message goes here)"
    lines = [
        "<b>Deployment</b>",
        html.escape(str(commit_message)),
        f"Execution: {html.escape(str(execution_id))}" if execution_id else "Execution: (execution ID here)",
        "",
    ]

    for key in STAGE_ORDER:
        label = STAGE_LABELS[key]
        stage = item["stages"].get(key, {"state": "PENDING", "ts": ""})
        emoji = _emoji_for_stage(stage.get("state"))
        ts = stage.get("ts") or "ts"
        if key in STAGE_SHOW_VERSIONS:
            v = versions.get(key, {})
            line = (
                f"{emoji} {label}: ({html.escape(str(ts))}) "
                f"S:{html.escape(str(v.get('S', 'n/a')))} | "
                f"F:{html.escape(str(v.get('F', 'n/a')))} | "
                f"C:{html.escape(str(v.get('C', 'n/a')))}"
            )
        else:
            line = f"{emoji} {label}"
        lines.append(line)

    if link:
        lines.append("")
        lines.append(f'<a href="{link}">Open Pipeline</a>')

    return "<br/>".join(lines)


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
    item = existing or {
        "execution_key": execution_key,
        "stages": _init_stages(),
    }
    if "stages" not in item:
        item["stages"] = _init_stages()
    if pipeline:
        item["pipeline"] = pipeline
    if execution_id:
        item["execution_id"] = execution_id

    commit_message = detail.get("commit-message") or detail.get("commitMessage") or detail.get("commit_message")
    if commit_message:
        item["commit_message"] = commit_message

    stage_key = _stage_from_event(detail)
    if stage_key:
        item["stages"][stage_key] = {
            "state": _pick_state(detail),
            "ts": event.get("time") or "",
        }

    content = _format_summary(item, versions, pipeline, execution_id, link)
    message_id = existing.get("message_id") if existing else None

    if message_id:
        url = f"{graph_base}/teams/{team_id}/channels/{channel_id}/messages/{message_id}"
        status, body = _graph_request("PATCH", url, token, {"body": {"contentType": "html", "content": content}})
        if status == 404:
            message_id = None
        elif status >= 400:
            return {"status": status, "error": body, "execution_key": execution_key}

    if not message_id:
        url = f"{graph_base}/teams/{team_id}/channels/{channel_id}/messages"
        status, body = _graph_request("POST", url, token, {"body": {"contentType": "html", "content": content}})
        if status >= 400:
            return {"status": status, "error": body, "execution_key": execution_key}
        message_id = body.get("id")

    if message_id:
        item["message_id"] = message_id
        item["updated_at"] = int(time.time())
        table.put_item(Item=item)

    return {
        "status": 200,
        "execution_key": execution_key,
        "message_id": message_id,
    }
