#!/bin/bash
set -euo pipefail

if [[ -z "${1:-}" ]]; then
  echo "Usage: $0 <staging|integration|production> [--dry-run]" >&2
  exit 1
fi

ENV_NAME=""
DRY_RUN="${DRY_RUN:-}"
while [[ $# -gt 0 ]]; do
  case "$1" in
    --dry-run)
      DRY_RUN="1"
      shift
      ;;
    *)
      if [[ -z "$ENV_NAME" ]]; then
        ENV_NAME="$1"
      fi
      shift
      ;;
  esac
done
if [[ -z "$ENV_NAME" ]]; then
  echo "Env name is required." >&2
  exit 1
fi
if [[ "$ENV_NAME" == "development" ]]; then
  echo "Development is not supported for cert attachment." >&2
  exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
DOMAINS_FILE="${DOMAINS_FILE:-$SCRIPT_DIR/domains.env}"

if [[ ! -f "$DOMAINS_FILE" ]]; then
  echo "Domains file not found: $DOMAINS_FILE" >&2
  exit 1
fi

# shellcheck source=/dev/null
source "$DOMAINS_FILE"

ENV_KEY="${ENV_NAME^^}"
FTS_DOMAIN_VAR="FTS_${ENV_KEY}"
CFS_DOMAIN_VAR="CFS_${ENV_KEY}"

FTS_DOMAIN="${!FTS_DOMAIN_VAR:-}"
CFS_DOMAIN="${!CFS_DOMAIN_VAR:-}"

if [[ -z "$FTS_DOMAIN" || -z "$CFS_DOMAIN" ]]; then
  echo "Failed to resolve domains for env '$ENV_NAME' from locals files." >&2
  echo "FTS_DOMAIN='$FTS_DOMAIN' CFS_DOMAIN='$CFS_DOMAIN'" >&2
  exit 1
fi

AWS_REGION="${AWS_REGION:-eu-west-2}"
AWS_CMD="${AWS_CMD:-}"
FTS_LB_NAME="${FTS_LB_NAME:-cdp-sirsi-php}"
CFS_LB_NAME="${CFS_LB_NAME:-cdp-sirsi-php}"
CERT_ARN_FTS="${CERT_ARN_FTS:-}"
CERT_ARN_CFS="${CERT_ARN_CFS:-}"

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

resolve_listener_arn_443() {
  local lb_name="$1"
  local lb_arn
  lb_arn=$($AWS_CMD elbv2 describe-load-balancers \
    --region "$AWS_REGION" \
    --query "LoadBalancers[?LoadBalancerName=='${lb_name}'].LoadBalancerArn" \
    --output text)

  if [[ -z "$lb_arn" || "$lb_arn" == "None" ]]; then
    echo "Load balancer not found: $lb_name" >&2
    return 1
  fi

  local listener_arn
  listener_arn=$($AWS_CMD elbv2 describe-listeners \
    --region "$AWS_REGION" \
    --load-balancer-arn "$lb_arn" \
    --query 'Listeners[?Port==`443`].ListenerArn' \
    --output text)

  if [[ -z "$listener_arn" || "$listener_arn" == "None" ]]; then
    echo "HTTPS:443 listener not found for $lb_name" >&2
    return 1
  fi

  echo "$listener_arn"
}

find_latest_cert_arn() {
  local domain="$1"
  AWS_CMD="$AWS_CMD" AWS_REGION="$AWS_REGION" DOMAIN="$domain" "$PYTHON_BIN" - <<'PY'
import json
import os
import subprocess
import sys
from datetime import datetime
import shlex

aws_cmd = os.environ["AWS_CMD"]
region = os.environ["AWS_REGION"]
domain = os.environ["DOMAIN"]

def run(cmd_args):
    p = subprocess.run(cmd_args, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    if p.returncode != 0:
        sys.stderr.write(p.stderr)
        raise SystemExit(p.returncode)
    return p.stdout

aws_parts = shlex.split(aws_cmd)
lst = run(aws_parts + ["acm", "list-certificates", "--region", region, "--output", "json"])
data = json.loads(lst)
matches = [c for c in data.get("CertificateSummaryList", []) if c.get("DomainName") == domain]

if not matches:
    sys.exit(1)

best = None
for m in matches:
    arn = m.get("CertificateArn")
    if not arn:
        continue
    desc_raw = run(aws_parts + ["acm", "describe-certificate", "--region", region, "--certificate-arn", arn, "--output", "json"])
    desc = json.loads(desc_raw).get("Certificate", {})
    if desc.get("Status") != "ISSUED":
        continue
    not_after = desc.get("NotAfter")
    if not not_after:
        continue
    # NotAfter is ISO string
    ts = datetime.fromisoformat(not_after.replace("Z", "+00:00"))
    if best is None or ts > best[0]:
        best = (ts, arn)

if best is None:
    sys.exit(1)

print(best[1])
PY
}

attach_cert() {
  local domain="$1"
  local lb_name="$2"
  local listener_arn
  listener_arn="$(resolve_listener_arn_443 "$lb_name")"
  local cert_arn="${3:-}"
  if [[ -z "$cert_arn" ]]; then
    cert_arn="$(find_latest_cert_arn "$domain" || true)"
  fi

  if [[ -z "$cert_arn" ]]; then
    echo "No ISSUED cert found in ACM for domain: $domain (or list not permitted)" >&2
    exit 1
  fi

  if [[ "$DRY_RUN" == "1" ]]; then
    echo "DRY_RUN=1: domain=$domain listener_arn=$listener_arn cert_arn=$cert_arn"
    return 0
  fi

  if $AWS_CMD elbv2 describe-listener-certificates \
    --region "$AWS_REGION" \
    --listener-arn "$listener_arn" \
    --query "Certificates[?CertificateArn=='${cert_arn}'].CertificateArn" \
    --output text | grep -q .; then
    echo "Already attached: $domain ($cert_arn)"
    return 0
  fi

  $AWS_CMD elbv2 add-listener-certificates \
    --region "$AWS_REGION" \
    --listener-arn "$listener_arn" \
    --certificates CertificateArn="$cert_arn" >/dev/null

  echo "Attached: $domain ($cert_arn)"
}

attach_cert "$FTS_DOMAIN" "$FTS_LB_NAME" "$CERT_ARN_FTS"
attach_cert "$CFS_DOMAIN" "$CFS_LB_NAME" "$CERT_ARN_CFS"

echo "Done. Attached certs for $ENV_NAME"
