#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "${SCRIPT_DIR}/../grafana/terraform" && pwd)"

# shellcheck source=grafana-env.sh
source "${SCRIPT_DIR}/grafana-env.sh"

ACCOUNT_ID="${TF_VAR_cloudwatch_account_id}"
ENVIRONMENT="${TG_ENVIRONMENT:-development}"

auth_header() {
  if [[ "${TF_VAR_grafana_token}" == Bearer\ * ]]; then
    printf '%s' "Authorization: ${TF_VAR_grafana_token}"
  else
    printf '%s' "Authorization: Bearer ${TF_VAR_grafana_token}"
  fi
}

echo "Grafana apply:"
echo "  environment=${ENVIRONMENT}"
echo "  aws_account_id=${ACCOUNT_ID}"
echo "  grafana_url=${TF_VAR_grafana_url}"

if command -v curl >/dev/null 2>&1; then
  set +e
  CURL_OUT="$(curl -sS -o /tmp/grafana-auth-check.json -w "%{http_code}" \
    -H "$(auth_header)" \
    "${TF_VAR_grafana_url%/}/api/user")"
  CURL_STATUS=$?
  set -e

  if [ "${CURL_STATUS}" -ne 0 ] || [ "${CURL_OUT}" -ge 400 ]; then
    echo "ERROR: Grafana API auth check failed (http=${CURL_OUT}, curl_exit=${CURL_STATUS})." >&2
    if [ -f /tmp/grafana-auth-check.json ]; then
      echo "Response:" >&2
      sed -n '1,50p' /tmp/grafana-auth-check.json >&2 || true
    fi
    cat >&2 <<'EOF'

Likely causes:
  - The Grafana API token is missing/invalid (common after deleting/recreating the Grafana DB).
EOF
    exit 1
  fi
fi

cd "${ROOT_DIR}"

terraform init -input=false -reconfigure \
  -backend-config="bucket=tfstate-cdp-sirsi-${ENVIRONMENT}-${ACCOUNT_ID}" \
  -backend-config="key=tools/grafana/terraform/terraform.tfstate" \
  -backend-config="region=eu-west-2" \
  -backend-config="encrypt=true" \
  -backend-config="use_lockfile=true"

terraform apply -input=false -var-file="env/${ENVIRONMENT}.tfvars"
