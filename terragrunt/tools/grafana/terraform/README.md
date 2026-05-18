# Grafana Terraform (configuration)

This folder provisions Grafana configuration via the Grafana API.
It is intentionally separate from the core infra Terraform so changes can be applied independently.

Primary deployment documentation (container): `terragrunt/docs/grafana.md`.

## Prerequisites

- A Grafana **API token** per environment (stored in Secrets Manager):
  - Secret name (per account): `cdp-sirsi-grafana-api-token`
  - Secret JSON keys:
    - `API_TOKEN`

- Teams webhook (optional):
  - Secret name: `cdp-sirsi-grafana-alerting-webhook`
  - Secret JSON key:
    - `TEAMS_WEBHOOK_URL`

- GitHub OIDC role must have S3 access to the state bucket:
  - Bucket: `tfstate-cdp-sirsi-<env>-<account_id>`
  - Actions: `s3:ListBucket`, `s3:GetBucketLocation`, `s3:GetObject`, `s3:PutObject`, `s3:DeleteObject`

## Dashboard provisioning

Place Grafana dashboard JSON files under `dashboards/`, grouped by folder:
- `dashboards/application`
- `dashboards/infrastructure`
- `dashboards/overview`
- `dashboards/traffic`

Each `*.json` file is provisioned into the matching folder.
Dashboard UIDs are generated deterministically from the folder + filename and truncated to 40 chars,
so URLs are stable across environments.

## Local usage

```bash
ACCOUNT_ID="$(ave aws sts get-caller-identity --query Account --output text)"
GRAFANA_API_TOKEN="$(ave aws secretsmanager get-secret-value \
  --secret-id cdp-sirsi-grafana-api-token \
  --query SecretString --output text | jq -r '.API_TOKEN')"
GRAFANA_TEAMS_WEBHOOK_URL="$(ave aws secretsmanager get-secret-value \
  --secret-id cdp-sirsi-grafana-alerting-webhook \
  --query SecretString --output text 2>/dev/null | jq -r '.TEAMS_WEBHOOK_URL')"

if [ "${TG_ENVIRONMENT:-development}" = "production" ]; then
  TF_VAR_grafana_url="https://grafana.supplier-information.find-tender.service.gov.uk"
elif [ "${TG_ENVIRONMENT:-development}" = "development" ]; then
  TF_VAR_grafana_url="https://grafana.dev.supplier-information.find-tender.service.gov.uk"
else
  TF_VAR_grafana_url="https://grafana.${TG_ENVIRONMENT:-development}.supplier-information.find-tender.service.gov.uk"
fi
export TF_VAR_grafana_url

export TF_VAR_grafana_token="${GRAFANA_API_TOKEN}"
export TF_VAR_teams_webhook_url="${GRAFANA_TEAMS_WEBHOOK_URL:-}"
export TF_VAR_environment="${TG_ENVIRONMENT:-development}"
export TF_VAR_cloudwatch_account_id="${ACCOUNT_ID}"
export TF_VAR_cloudwatch_assume_role_arn="arn:aws:iam::${TF_VAR_cloudwatch_account_id}:role/cdp-sirsi-telemetry"
```

```bash
ave terraform init -input=false -reconfigure \
  -backend-config="bucket=tfstate-cdp-sirsi-${TG_ENVIRONMENT:-development}-${ACCOUNT_ID}" \
  -backend-config="key=tools/grafana/terraform/terraform.tfstate" \
  -backend-config="region=eu-west-2" \
  -backend-config="encrypt=true" \
  -backend-config="use_lockfile=true"
ave terraform plan -input=false -var-file=env/${TG_ENVIRONMENT:-development}.tfvars
ave terraform apply -input=false -var-file=env/${TG_ENVIRONMENT:-development}.tfvars
```

## Notes

- Alert rules and notification policies can be added incrementally once we decide how to structure them.
- This approach avoids baking Grafana config into the Docker image.

## CI/CD

GitHub Actions workflow: `.github/workflows/grafana-*.yml`

- Runs on `main` when files under `terragrunt/tools/grafana/**` change
- Uses OIDC to assume role `cdp-sirsi-terraform` in each account
