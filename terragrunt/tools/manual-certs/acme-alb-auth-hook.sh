#!/bin/bash
set -euo pipefail

if [[ -z "${HOOK_ENV:-}" ]]; then
  echo "HOOK_ENV not set; expected path to env file" >&2
  exit 1
fi

# shellcheck source=/dev/null
source "$HOOK_ENV"

: "${AWS_REGION:?AWS_REGION is required}"

if [[ -z "${AWS_CMD:-}" ]]; then
  if command -v ave >/dev/null 2>&1; then
    AWS_CMD="$(command -v ave) aws"
  elif command -v aws >/dev/null 2>&1; then
    AWS_CMD="$(command -v aws)"
  else
    echo "Neither 'ave' nor 'aws' found in PATH; set AWS_CMD explicitly" >&2
    exit 1
  fi
fi

ENV_DIR="$(cd "$(dirname "$HOOK_ENV")" && pwd)"
STATE_DIR="${STATE_DIR:-$ENV_DIR/state}"
mkdir -p "$STATE_DIR"

if [[ -z "${LISTENER_ARN:-}" ]]; then
  SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
  LISTENER_ARN="$("$SCRIPT_DIR/resolve-listener-arn.sh" "$HOOK_ENV")"
fi

: "${LISTENER_ARN:?LISTENER_ARN is required}"

HOST_HEADER="${ACME_HOST_HEADER:-${CERTBOT_DOMAIN}}"
TOKEN="${CERTBOT_TOKEN}"
VALIDATION="${CERTBOT_VALIDATION}"

export ACME_HOST_HEADER="$HOST_HEADER"

PYTHON_BIN="${PYTHON_BIN:-}"
if [[ -z "$PYTHON_BIN" ]]; then
  if command -v python3 >/dev/null 2>&1; then
    PYTHON_BIN="$(command -v python3)"
  elif command -v python >/dev/null 2>&1; then
    PYTHON_BIN="$(command -v python)"
  else
    echo "Neither 'python3' nor 'python' found in PATH; set PYTHON_BIN explicitly" >&2
    exit 1
  fi
fi

CONDITIONS=$("$PYTHON_BIN" - <<'PY'
import json, os
host = os.environ["ACME_HOST_HEADER"]
token = os.environ["CERTBOT_TOKEN"]
print(json.dumps([
  {"Field": "host-header", "HostHeaderConfig": {"Values": [host]}},
  {"Field": "path-pattern", "PathPatternConfig": {"Values": [f"/.well-known/acme-challenge/{token}"]}},
]))
PY
)

ACTIONS=$("$PYTHON_BIN" - <<'PY'
import json, os
print(json.dumps([
  {
    "Type": "fixed-response",
    "FixedResponseConfig": {
      "StatusCode": "200",
      "ContentType": "text/plain",
      "MessageBody": os.environ["CERTBOT_VALIDATION"],
    },
  }
]))
PY
)

RULE_ARN=""
PRIORITY="${ACME_PRIORITY:-}"
if [[ -z "$PRIORITY" ]]; then
  RULES_JSON=$($AWS_CMD elbv2 describe-rules \
    --listener-arn "$LISTENER_ARN" \
    --region "$AWS_REGION" \
    --output json)

  export RULES_JSON
  PRIORITY=$("$PYTHON_BIN" - <<'PY'
import json, os, sys, fnmatch

rules = json.loads(os.environ["RULES_JSON"]).get("Rules", [])
used = set()
host = os.environ["ACME_HOST_HEADER"].lower()
min_host_priority = None

for r in rules:
    p = r.get("Priority")
    if p and p != "default":
        try:
            used.add(int(p))
        except ValueError:
            pass
    # Find rules that already match this host header
    for cond in r.get("Conditions", []):
        if cond.get("Field") == "host-header":
            vals = [v.lower() for v in cond.get("HostHeaderConfig", {}).get("Values", [])]
            if host in vals or any(fnmatch.fnmatch(host, v) for v in vals if "*" in v):
                if p and p != "default":
                    try:
                        ip = int(p)
                        if min_host_priority is None or ip < min_host_priority:
                            min_host_priority = ip
                    except ValueError:
                        pass

if min_host_priority == 1:
    print("", end="")
    sys.exit(0)

def first_free(max_priority):
    for candidate in range(1, max_priority + 1):
        if candidate not in used:
            return candidate
    return None

if min_host_priority:
    # choose the highest available priority below the existing host rule
    for candidate in range(min_host_priority - 1, 0, -1):
        if candidate not in used:
            print(candidate, end="")
            sys.exit(0)

priority = first_free(50000)
print(priority if priority is not None else "", end="")
PY
  )
fi

if [[ -z "$PRIORITY" ]]; then
  echo "Unable to choose a listener rule priority (host rule may already be priority 1)." >&2
  exit 1
fi

for _ in {1..5}; do
  set +e
  RULE_ARN=$($AWS_CMD elbv2 create-rule \
    --listener-arn "$LISTENER_ARN" \
    --priority "$PRIORITY" \
    --conditions "$CONDITIONS" \
    --actions "$ACTIONS" \
    --region "$AWS_REGION" \
    --query 'Rules[0].RuleArn' \
    --output text 2>/dev/null)
  STATUS=$?
  set -e
  if [[ $STATUS -eq 0 && -n "$RULE_ARN" && "$RULE_ARN" != "None" ]]; then
    break
  fi
  RULE_ARN=""
  if [[ "$PRIORITY" -gt 1 ]]; then
    PRIORITY=$((PRIORITY - 1))
  fi
  sleep 1

done

if [[ -z "$RULE_ARN" ]]; then
  echo "Failed to create ALB rule after retries" >&2
  exit 1
fi

STATE_FILE="$STATE_DIR/${CERTBOT_DOMAIN}-${CERTBOT_TOKEN}.rule_arn"
echo "$RULE_ARN" > "$STATE_FILE"

echo "Created ALB rule: $RULE_ARN"

if [[ "${VERIFY_CHALLENGE:-}" == "1" ]]; then
  if [[ -z "${LOAD_BALANCER_NAME:-}" ]]; then
    echo "VERIFY_CHALLENGE=1 set but LOAD_BALANCER_NAME is empty; skipping verify" >&2
    exit 0
  fi

  LB_DNS=$($AWS_CMD elbv2 describe-load-balancers \
    --region "$AWS_REGION" \
    --query "LoadBalancers[?LoadBalancerName=='${LOAD_BALANCER_NAME}'].DNSName" \
    --output text)

  if [[ -z "$LB_DNS" || "$LB_DNS" == "None" ]]; then
    echo "Failed to resolve LB DNS for $LOAD_BALANCER_NAME; skipping verify" >&2
    exit 0
  fi

  VERIFY_URL="http://$LB_DNS/.well-known/acme-challenge/$TOKEN"
  RESPONSE=""
  for _ in {1..15}; do
    RESPONSE=$(curl -sS -H "Host: $HOST_HEADER" "$VERIFY_URL" || true)
    if [[ "$RESPONSE" == "$VALIDATION" ]]; then
      echo "Challenge verify OK: $VERIFY_URL"
      exit 0
    fi
    sleep 2
  done

  echo "Challenge verify failed. Expected '${VALIDATION}', got '${RESPONSE}'" >&2
  echo "Verify URL: $VERIFY_URL (Host: $HOST_HEADER)" >&2
  exit 1
fi
