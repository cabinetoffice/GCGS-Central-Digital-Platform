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

STATE_FILE="$STATE_DIR/${CERTBOT_DOMAIN}-${CERTBOT_TOKEN}.rule_arn"

if [[ ! -f "$STATE_FILE" ]]; then
  echo "No state file found for rule cleanup: $STATE_FILE" >&2
  exit 0
fi

RULE_ARN=$(cat "$STATE_FILE")

if [[ "${KEEP_RULE:-}" == "1" ]]; then
  echo "KEEP_RULE=1 set; skipping rule deletion for $RULE_ARN"
  exit 0
fi

if [[ -n "$RULE_ARN" ]]; then
  $AWS_CMD elbv2 delete-rule --rule-arn "$RULE_ARN" --region "$AWS_REGION" || true
  echo "Deleted ALB rule: $RULE_ARN"
fi

rm -f "$STATE_FILE"
