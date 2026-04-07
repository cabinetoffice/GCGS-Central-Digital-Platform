#!/bin/bash
set -euo pipefail

if [[ -z "${1:-}" ]]; then
  echo "Usage: $0 <staging|integration|production> [--dry-run] [--label <text>]" >&2
  exit 1
fi

ENV_NAME=""
DRY_RUN="${DRY_RUN:-}"
LABEL=""
while [[ $# -gt 0 ]]; do
  case "$1" in
    --dry-run)
      DRY_RUN="1"
      shift
      ;;
    --label)
      LABEL="${2:-}"
      shift 2
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
  echo "Development is not supported for cert validation." >&2
  exit 1
fi

if [[ -n "${LOG_FILE:-}" ]]; then
  mkdir -p "$(dirname "$LOG_FILE")"
  exec > >(tee -a "$LOG_FILE") 2>&1
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

AWS_REGION="${AWS_REGION:-eu-west-2}"
AWS_CMD="${AWS_CMD:-}"
CERT_ARN_FTS="${CERT_ARN_FTS:-}"
CERT_ARN_CFS="${CERT_ARN_CFS:-}"
LIVE_WAIT_SECONDS="${LIVE_WAIT_SECONDS:-120}"
LIVE_WAIT_INTERVAL="${LIVE_WAIT_INTERVAL:-5}"
SUMMARY_FILE="${SUMMARY_FILE:-}"
PREV_LIVE_SERIAL_FTS="${PREV_LIVE_SERIAL_FTS:-}"
PREV_LIVE_SERIAL_CFS="${PREV_LIVE_SERIAL_CFS:-}"
PREV_LIVE_NOT_AFTER_FTS="${PREV_LIVE_NOT_AFTER_FTS:-}"
PREV_LIVE_NOT_AFTER_CFS="${PREV_LIVE_NOT_AFTER_CFS:-}"

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

can_list_certs() {
  $AWS_CMD acm list-certificates --region "$AWS_REGION" --output json >/dev/null 2>&1
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
        # suppress noisy errors; caller handles permissions
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
    ts = datetime.fromisoformat(not_after.replace("Z", "+00:00"))
    if best is None or ts > best[0]:
        best = (ts, arn)

if best is None:
    sys.exit(1)

print(best[1])
PY
}

describe_cert() {
  local cert_arn="$1"
  $AWS_CMD acm describe-certificate --region "$AWS_REGION" --certificate-arn "$cert_arn" --output json
}

print_live_cert() {
  local domain="$1"
  if ! command -v openssl >/dev/null 2>&1; then
    echo "  LIVE: openssl not found" >&2
    return 0
  fi

  local s_client_cmd
  if command -v timeout >/dev/null 2>&1; then
    s_client_cmd=(timeout 10s openssl s_client -servername "$domain" -connect "$domain:443" -showcerts)
  else
    s_client_cmd=(openssl s_client -servername "$domain" -connect "$domain:443" -showcerts)
  fi

  local cert_out
  cert_out="$("${s_client_cmd[@]}" </dev/null 2>/dev/null | openssl x509 -noout -subject -issuer -dates -serial -fingerprint -sha256 2>/dev/null || true)"
  if [[ -z "$cert_out" ]]; then
    echo "  LIVE: unable to fetch cert" >&2
    return 0
  fi
  echo "$cert_out" | sed 's/^/  LIVE: /'
}

fetch_live_serial() {
  local domain="$1"
  if ! command -v openssl >/dev/null 2>&1; then
    return 1
  fi
  local s_client_cmd
  if command -v timeout >/dev/null 2>&1; then
    s_client_cmd=(timeout 10s openssl s_client -servername "$domain" -connect "$domain:443" -showcerts)
  else
    s_client_cmd=(openssl s_client -servername "$domain" -connect "$domain:443" -showcerts)
  fi
  local serial
  serial="$("${s_client_cmd[@]}" </dev/null 2>/dev/null | openssl x509 -noout -serial 2>/dev/null | sed -e 's/^serial=//' -e 's/://g' | tr 'A-F' 'a-f' || true)"
  if [[ -z "$serial" ]]; then
    return 1
  fi
  echo "$serial"
}

fetch_live_not_after() {
  local domain="$1"
  if ! command -v openssl >/dev/null 2>&1; then
    return 1
  fi
  local s_client_cmd
  if command -v timeout >/dev/null 2>&1; then
    s_client_cmd=(timeout 10s openssl s_client -servername "$domain" -connect "$domain:443" -showcerts)
  else
    s_client_cmd=(openssl s_client -servername "$domain" -connect "$domain:443" -showcerts)
  fi
  local not_after
  not_after="$("${s_client_cmd[@]}" </dev/null 2>/dev/null | openssl x509 -noout -enddate 2>/dev/null | sed -e 's/^notAfter=//' || true)"
  if [[ -z "$not_after" ]]; then
    return 1
  fi
  echo "$not_after"
}

LIVE_SERIAL=""
LIVE_NOT_AFTER=""
wait_for_live_cert() {
  local domain="$1"
  local expected_serial="$2"
  local prev_serial="$3"
  local prev_not_after="$4"

  local waited=0
  local live_serial=""
  while [[ "$waited" -le "$LIVE_WAIT_SECONDS" ]]; do
    live_serial="$(fetch_live_serial "$domain" || true)"
    if [[ -n "$expected_serial" && -n "$live_serial" && "$live_serial" == "$expected_serial" ]]; then
      LIVE_SERIAL="$live_serial"
      LIVE_NOT_AFTER="$(fetch_live_not_after "$domain" || true)"
      print_live_cert "$domain"
      return 0
    fi
    if [[ -n "$prev_serial" && -n "$live_serial" && "$live_serial" != "$prev_serial" ]]; then
      LIVE_SERIAL="$live_serial"
      LIVE_NOT_AFTER="$(fetch_live_not_after "$domain" || true)"
      print_live_cert "$domain"
      return 0
    fi
    if [[ -n "$prev_not_after" ]]; then
      local live_not_after
      live_not_after="$(fetch_live_not_after "$domain" || true)"
      if [[ -n "$live_not_after" && "$live_not_after" != "$prev_not_after" ]]; then
        LIVE_SERIAL="$live_serial"
        LIVE_NOT_AFTER="$live_not_after"
        print_live_cert "$domain"
        return 0
      fi
    fi
    if [[ "$waited" -eq 0 ]]; then
      echo "  LIVE: waiting for new cert to appear (serial mismatch)"
    fi
    sleep "$LIVE_WAIT_INTERVAL"
    waited=$((waited + LIVE_WAIT_INTERVAL))
  done

  LIVE_SERIAL="$live_serial"
  LIVE_NOT_AFTER="$(fetch_live_not_after "$domain" || true)"
  print_live_cert "$domain"
  return 1
}

ACM_SERIAL=""
ACM_NOT_AFTER=""
FTS_ACM_SERIAL=""
FTS_ACM_NOT_AFTER=""
CFS_ACM_SERIAL=""
CFS_ACM_NOT_AFTER=""
FTS_LIVE_SERIAL=""
FTS_LIVE_NOT_AFTER=""
CFS_LIVE_SERIAL=""
CFS_LIVE_NOT_AFTER=""
print_acm_cert() {
  local domain="$1"
  local arn_override="${2:-}"
  local arn=""

  if [[ -n "$arn_override" ]]; then
    arn="$arn_override"
  else
    if ! can_list_certs; then
      echo "  ACM: list not permitted (skipping ACM lookup)"
      return 0
    fi
    arn="$(find_latest_cert_arn "$domain" || true)"
  fi

  if [[ -z "$arn" ]]; then
    echo "  ACM: no ISSUED cert found"
    return 0
  fi
  local desc
  if ! desc="$(describe_cert "$arn" 2>/dev/null)"; then
    echo "  ACM: describe not permitted for $arn"
    return 0
  fi
  AWS_CERT_JSON="$desc" "$PYTHON_BIN" - <<'PY'
import json, os
data = json.loads(os.environ["AWS_CERT_JSON"])
cert = data.get("Certificate", {})
print(f"  ACM: arn={cert.get('CertificateArn')}")
print(f"  ACM: domain={cert.get('DomainName')}")
print(f"  ACM: status={cert.get('Status')}")
print(f"  ACM: not_after={cert.get('NotAfter')}")
print(f"  ACM: serial={cert.get('Serial')}")
PY

  ACM_SERIAL="$(AWS_CERT_JSON="$desc" "$PYTHON_BIN" - <<'PY'
import json, os
data = json.loads(os.environ["AWS_CERT_JSON"])
cert = data.get("Certificate", {})
serial = (cert.get("Serial") or "").replace(":", "").lower()
print(serial)
PY
  )"
  ACM_NOT_AFTER="$(AWS_CERT_JSON="$desc" "$PYTHON_BIN" - <<'PY'
import json, os
data = json.loads(os.environ["AWS_CERT_JSON"])
cert = data.get("Certificate", {})
print(cert.get("NotAfter") or "")
PY
  )"
}

if [[ -n "$LABEL" ]]; then
  echo "=== Certificate validation ($LABEL) for $ENV_NAME ==="
else
  echo "=== Certificate validation for $ENV_NAME ==="
fi

FAILED=0
for domain in "$FTS_DOMAIN" "$CFS_DOMAIN"; do
  echo ""
  echo "Domain: $domain"
  if [[ "$DRY_RUN" == "1" ]]; then
    echo "  DRY_RUN=1: skipping ACM + live checks"
    continue
  fi
  if [[ "$domain" == "$FTS_DOMAIN" ]]; then
    print_acm_cert "$domain" "$CERT_ARN_FTS"
    FTS_ACM_SERIAL="$ACM_SERIAL"
    FTS_ACM_NOT_AFTER="$ACM_NOT_AFTER"
    if ! wait_for_live_cert "$domain" "$ACM_SERIAL" "$PREV_LIVE_SERIAL_FTS" "$PREV_LIVE_NOT_AFTER_FTS"; then
      FAILED=1
    fi
    FTS_LIVE_SERIAL="$LIVE_SERIAL"
    FTS_LIVE_NOT_AFTER="$LIVE_NOT_AFTER"
  else
    print_acm_cert "$domain" "$CERT_ARN_CFS"
    CFS_ACM_SERIAL="$ACM_SERIAL"
    CFS_ACM_NOT_AFTER="$ACM_NOT_AFTER"
    if ! wait_for_live_cert "$domain" "$ACM_SERIAL" "$PREV_LIVE_SERIAL_CFS" "$PREV_LIVE_NOT_AFTER_CFS"; then
      FAILED=1
    fi
    CFS_LIVE_SERIAL="$LIVE_SERIAL"
    CFS_LIVE_NOT_AFTER="$LIVE_NOT_AFTER"
  fi
done

echo ""
if [[ -n "$SUMMARY_FILE" ]]; then
  mkdir -p "$(dirname "$SUMMARY_FILE")"
  {
    printf 'FTS_DOMAIN=%q\n' "${FTS_DOMAIN}"
    printf 'CFS_DOMAIN=%q\n' "${CFS_DOMAIN}"
    printf 'FTS_ACM_SERIAL=%q\n' "${FTS_ACM_SERIAL}"
    printf 'FTS_ACM_NOT_AFTER=%q\n' "${FTS_ACM_NOT_AFTER}"
    printf 'CFS_ACM_SERIAL=%q\n' "${CFS_ACM_SERIAL}"
    printf 'CFS_ACM_NOT_AFTER=%q\n' "${CFS_ACM_NOT_AFTER}"
    printf 'FTS_LIVE_SERIAL=%q\n' "${FTS_LIVE_SERIAL}"
    printf 'FTS_LIVE_NOT_AFTER=%q\n' "${FTS_LIVE_NOT_AFTER}"
    printf 'CFS_LIVE_SERIAL=%q\n' "${CFS_LIVE_SERIAL}"
    printf 'CFS_LIVE_NOT_AFTER=%q\n' "${CFS_LIVE_NOT_AFTER}"
  } > "$SUMMARY_FILE"
fi

if [[ -n "$PREV_LIVE_SERIAL_FTS" || -n "$PREV_LIVE_SERIAL_CFS" || -n "$PREV_LIVE_NOT_AFTER_FTS" || -n "$PREV_LIVE_NOT_AFTER_CFS" ]]; then
  echo "Delta (live):"
  if [[ -n "$PREV_LIVE_SERIAL_FTS" || -n "$PREV_LIVE_NOT_AFTER_FTS" ]]; then
    echo "  FTS serial: ${PREV_LIVE_SERIAL_FTS:-<none>} -> ${FTS_LIVE_SERIAL:-<none>}"
    echo "  FTS notAfter: ${PREV_LIVE_NOT_AFTER_FTS:-<none>} -> ${FTS_LIVE_NOT_AFTER:-<none>}"
  fi
  if [[ -n "$PREV_LIVE_SERIAL_CFS" || -n "$PREV_LIVE_NOT_AFTER_CFS" ]]; then
    echo "  CFS serial: ${PREV_LIVE_SERIAL_CFS:-<none>} -> ${CFS_LIVE_SERIAL:-<none>}"
    echo "  CFS notAfter: ${PREV_LIVE_NOT_AFTER_CFS:-<none>} -> ${CFS_LIVE_NOT_AFTER:-<none>}"
  fi
  echo ""
fi

if [[ "$FAILED" -ne 0 ]]; then
  echo "Done (live cert mismatch after waiting)."
  exit 1
fi
echo "Done."
