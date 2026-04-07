#!/bin/bash
set -euo pipefail

if [[ -z "${1:-}" ]]; then
  echo "Usage: $0 <staging|integration|production> [--dry-run] [--attach]" >&2
  exit 1
fi

ENV_NAME=""
DRY_RUN="${DRY_RUN:-}"
ATTACH="${ATTACH:-}"
while [[ $# -gt 0 ]]; do
  case "$1" in
    --dry-run)
      DRY_RUN="1"
      shift
      ;;
    --attach)
      ATTACH="1"
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
  echo "Development is not supported for cert rotation." >&2
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
EMAIL="${EMAIL:-ali.bahman@goaco.com}"
AWS_CMD="${AWS_CMD:-}"
VERIFY_CHALLENGE="${VERIFY_CHALLENGE:-1}"
KEEP_RULE="${KEEP_RULE:-}"
ACME_SERVER="${ACME_SERVER:-}"

# Default to production CA unless explicitly overridden.

FTS_LB_NAME="${FTS_LB_NAME:-cdp-sirsi-php}"
CFS_LB_NAME="${CFS_LB_NAME:-cdp-sirsi-php}"

WORKDIR="$SCRIPT_DIR/work/manual-certs-${ENV_NAME}-$$"
mkdir -p "$WORKDIR"

FTS_ENV="$WORKDIR/fts.env.sh"
CFS_ENV="$WORKDIR/cfs.env.sh"

cat > "$FTS_ENV" <<EOF_INNER
DOMAIN="$FTS_DOMAIN"
LOAD_BALANCER_NAME="$FTS_LB_NAME"
AWS_REGION="$AWS_REGION"
EMAIL="$EMAIL"
EOF_INNER

cat > "$CFS_ENV" <<EOF_INNER
DOMAIN="$CFS_DOMAIN"
LOAD_BALANCER_NAME="$CFS_LB_NAME"
AWS_REGION="$AWS_REGION"
EMAIL="$EMAIL"
EOF_INNER

if [[ -n "$AWS_CMD" ]]; then
  export AWS_CMD
fi
if [[ -n "$VERIFY_CHALLENGE" ]]; then
  export VERIFY_CHALLENGE
fi
if [[ -n "$KEEP_RULE" ]]; then
  export KEEP_RULE
fi
if [[ -n "$ACME_SERVER" ]]; then
  export ACME_SERVER
fi
if [[ "$DRY_RUN" == "1" ]]; then
  DRY_RUN=1 "$SCRIPT_DIR/issue-cert.sh" "$FTS_ENV" --dry-run
  DRY_RUN=1 "$SCRIPT_DIR/issue-cert.sh" "$CFS_ENV" --dry-run
else
  CERT_ARN_FILE="$WORKDIR/fts.cert_arn" "$SCRIPT_DIR/issue-cert.sh" "$FTS_ENV"
  CERT_ARN_FILE="$WORKDIR/cfs.cert_arn" "$SCRIPT_DIR/issue-cert.sh" "$CFS_ENV"
fi

if [[ -n "$ATTACH" ]]; then
  if [[ "$DRY_RUN" == "1" ]]; then
    DRY_RUN=1 "$SCRIPT_DIR/attach-certs.sh" "$ENV_NAME" --dry-run
  else
    FTS_CERT_ARN="$(cat "$WORKDIR/fts.cert_arn" 2>/dev/null || true)"
    CFS_CERT_ARN="$(cat "$WORKDIR/cfs.cert_arn" 2>/dev/null || true)"
    CERT_ARN_FTS="$FTS_CERT_ARN" CERT_ARN_CFS="$CFS_CERT_ARN" "$SCRIPT_DIR/attach-certs.sh" "$ENV_NAME"
  fi
fi

echo "Done. Issued and imported certs for $ENV_NAME"
