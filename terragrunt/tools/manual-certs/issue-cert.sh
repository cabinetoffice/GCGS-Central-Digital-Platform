#!/bin/bash
set -euo pipefail

if [[ -z "${1:-}" ]]; then
  echo "Usage: $0 <env_name|path_to_env.sh> [--dry-run]" >&2
  exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"

ARG="$1"
DRY_RUN="${DRY_RUN:-}"
if [[ "${2:-}" == "--dry-run" ]]; then
  DRY_RUN="1"
fi
if [[ "$ARG" == *.sh || "$ARG" == */* ]]; then
  HOOK_ENV="$ARG"
else
  HOOK_ENV="$SCRIPT_DIR/$ARG/env.sh"
fi

if [[ ! -f "$HOOK_ENV" ]]; then
  echo "Env file not found: $HOOK_ENV" >&2
  exit 1
fi

# shellcheck source=/dev/null
source "$HOOK_ENV"

: "${DOMAIN:?DOMAIN is required}"
: "${EMAIL:?EMAIL is required}"
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
CERT_DIR="${CERT_DIR:-$ENV_DIR/certs}"
CERTBOT_DIR="${CERTBOT_DIR:-$ENV_DIR/certbot}"

mkdir -p "$STATE_DIR" "$CERT_DIR" "$CERTBOT_DIR"

# Allow multiple domains via DOMAINS (comma-separated). Fallback to DOMAIN.
DOMAINS_CSV="${DOMAINS:-$DOMAIN}"
IFS=',' read -r -a DOMAINS_ARR <<< "$DOMAINS_CSV"

CERTBOT_ARGS=(
  certonly
  --manual
  --preferred-challenges http
  --manual-auth-hook "$SCRIPT_DIR/acme-alb-auth-hook.sh"
  --manual-cleanup-hook "$SCRIPT_DIR/acme-alb-cleanup-hook.sh"
  --agree-tos
  --non-interactive
  --email "$EMAIL"
  --key-type "${KEY_TYPE:-rsa}"
  --rsa-key-size "${RSA_KEY_SIZE:-2048}"
  --config-dir "$CERTBOT_DIR/config"
  --work-dir "$CERTBOT_DIR/work"
  --logs-dir "$CERTBOT_DIR/logs"
)

if certbot --help 2>/dev/null | grep -q -- "--manual-public-ip-logging-ok"; then
  CERTBOT_ARGS+=( --manual-public-ip-logging-ok )
fi

# Allow overriding CA for testing (e.g. Let's Encrypt staging)
if [[ -n "${ACME_SERVER:-}" ]]; then
  CERTBOT_ARGS+=( --server "$ACME_SERVER" )
fi

for d in "${DOMAINS_ARR[@]}"; do
  d_trimmed="${d// /}"
  if [[ -n "$d_trimmed" ]]; then
    CERTBOT_ARGS+=( -d "$d_trimmed" )
  fi

done

export HOOK_ENV
if [[ -z "${LISTENER_ARN:-}" ]]; then
  LISTENER_ARN="$("$SCRIPT_DIR/resolve-listener-arn.sh" "$HOOK_ENV")"
  if [[ -z "$LISTENER_ARN" ]]; then
    echo "Failed to resolve LISTENER_ARN" >&2
    exit 1
  fi
fi

export LISTENER_ARN
export AWS_REGION
export AWS_CMD
export STATE_DIR

if [[ -z "${LISTENER_ARN:-}" ]]; then
  echo "LISTENER_ARN is still empty after resolution" >&2
  exit 1
fi

if [[ "$DRY_RUN" == "1" ]]; then
  echo "DRY_RUN=1: resolved domain and listener"
  echo "DOMAIN=$DOMAIN"
  echo "LISTENER_ARN=$LISTENER_ARN"
  exit 0
fi

certbot "${CERTBOT_ARGS[@]}"

# Copy certs to CERT_DIR for ACM import
PRIMARY_DOMAIN="${DOMAINS_ARR[0]// /}"
SRC_DIR="$CERTBOT_DIR/config/live/$PRIMARY_DOMAIN"

if [[ ! -d "$SRC_DIR" ]]; then
  echo "Expected certbot output directory not found: $SRC_DIR" >&2
  exit 1
fi

cp -f "$SRC_DIR/fullchain.pem" "$CERT_DIR/fullchain.pem"
cp -f "$SRC_DIR/privkey.pem" "$CERT_DIR/privkey.pem"

if [[ -n "${CERT_ARN_FILE:-}" ]]; then
  export CERT_ARN_FILE
fi
if [[ -n "${AWS_CMD:-}" ]]; then
  export AWS_CMD
fi

"$SCRIPT_DIR/letsencrypt.sh" "$CERT_DIR"

echo "Done. Imported certificate from $CERT_DIR"
