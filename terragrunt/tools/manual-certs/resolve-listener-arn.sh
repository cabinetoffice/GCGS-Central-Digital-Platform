#!/bin/bash
set -euo pipefail

if [[ -z "${1:-}" ]]; then
  echo "Usage: $0 <path_to_env.sh>" >&2
  exit 1
fi

HOOK_ENV="$1"
# Allow caller to override env file values
PRESET_LB_NAME="${LOAD_BALANCER_NAME:-}"
PRESET_LISTENER_ARN="${LISTENER_ARN:-}"
# shellcheck source=/dev/null
source "$HOOK_ENV"

if [[ -n "$PRESET_LB_NAME" ]]; then
  LOAD_BALANCER_NAME="$PRESET_LB_NAME"
fi
if [[ -n "$PRESET_LISTENER_ARN" ]]; then
  LISTENER_ARN="$PRESET_LISTENER_ARN"
fi

: "${AWS_REGION:?AWS_REGION is required}"
: "${LOAD_BALANCER_NAME:?LOAD_BALANCER_NAME is required when LISTENER_ARN is empty}"

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

LB_ARN=$($AWS_CMD elbv2 describe-load-balancers \
  --region "$AWS_REGION" \
  --query "LoadBalancers[?LoadBalancerName=='${LOAD_BALANCER_NAME}'].LoadBalancerArn" \
  --output text)

if [[ -z "$LB_ARN" || "$LB_ARN" == "None" ]]; then
  echo "Load balancer not found: $LOAD_BALANCER_NAME" >&2
  exit 1
fi

LISTENER_ARN=$($AWS_CMD elbv2 describe-listeners \
  --region "$AWS_REGION" \
  --load-balancer-arn "$LB_ARN" \
  --query 'Listeners[?Port==`80`].ListenerArn' \
  --output text)

if [[ -z "$LISTENER_ARN" || "$LISTENER_ARN" == "None" ]]; then
  echo "HTTP:80 listener not found for $LOAD_BALANCER_NAME" >&2
  exit 1
fi

echo "$LISTENER_ARN"
