#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "${SCRIPT_DIR}/../grafana/terraform" && pwd)"

# shellcheck source=grafana-env.sh
source "${SCRIPT_DIR}/grafana-env.sh"

ACCOUNT_ID="${TF_VAR_cloudwatch_account_id}"
ENVIRONMENT="${TG_ENVIRONMENT:-development}"

cd "${ROOT_DIR}"

terraform init -input=false -reconfigure \
  -backend-config="bucket=tfstate-cdp-sirsi-${ENVIRONMENT}-${ACCOUNT_ID}" \
  -backend-config="key=tools/grafana/terraform/terraform.tfstate" \
  -backend-config="region=eu-west-2" \
  -backend-config="encrypt=true" \
  -backend-config="use_lockfile=true"

terraform apply -input=false -var-file="env/${ENVIRONMENT}.tfvars"
