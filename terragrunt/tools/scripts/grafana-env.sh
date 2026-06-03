#!/usr/bin/env bash
set -euo pipefail

ACCOUNT_ID="$(aws sts get-caller-identity --query Account --output text)"
GRAFANA_API_TOKEN="$(aws secretsmanager get-secret-value \
  --secret-id cdp-sirsi-grafana-api-token \
  --query SecretString --output text | jq -r '.API_TOKEN')"

if [ "${TG_ENVIRONMENT:-development}" = "production" ]; then
  TF_VAR_grafana_url="https://grafana.supplier-information.find-tender.service.gov.uk"
elif [ "${TG_ENVIRONMENT:-development}" = "development" ]; then
  TF_VAR_grafana_url="https://grafana.dev.supplier-information.find-tender.service.gov.uk"
else
  TF_VAR_grafana_url="https://grafana.${TG_ENVIRONMENT:-development}.supplier-information.find-tender.service.gov.uk"
fi

export TF_VAR_grafana_url
export TF_VAR_grafana_token="${GRAFANA_API_TOKEN}"
export TF_VAR_environment="${TG_ENVIRONMENT:-development}"
export TF_VAR_cloudwatch_account_id="${ACCOUNT_ID}"
export TF_VAR_cloudwatch_assume_role_arn="arn:aws:iam::${TF_VAR_cloudwatch_account_id}:role/cdp-sirsi-telemetry"
