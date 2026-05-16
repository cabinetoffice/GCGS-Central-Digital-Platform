# Grafana Terraform (configuration)

This folder provisions Grafana configuration via the Grafana API (dashboards and alert contact points).
It is intentionally separate from the core infra Terraform so changes can be applied independently.

## Prerequisites

- A Grafana **API token** per environment (stored in Secrets Manager):
  - Secret name (per account): `cdp-sirsi-grafana-api-token`
  - Secret JSON keys:
    - `API_TOKEN`

- Teams webhook (optional):
  - Secret name: `cdp-sirsi-grafana-alerting-webhook`
  - Secret JSON key:
    - `TEAMS_WEBHOOK_URL`

## Dashboard provisioning

Place Grafana dashboard JSON files under `dashboards/`, grouped by folder:
- `dashboards/application`
- `dashboards/infrastructure`
- `dashboards/overview`
- `dashboards/traffic`

Each `*.json` file is provisioned into the matching folder.

## Local usage

```bash
ENVIRONMENT="${AWS_ENV:-${TG_ENVIRONMENT:-development}}"
if [ "${ENVIRONMENT}" = "production" ]; then
  TF_VAR_grafana_url="https://grafana.supplier-information.find-tender.service.gov.uk"
elif [ "${ENVIRONMENT}" = "development" ]; then
  TF_VAR_grafana_url="https://grafana.dev.supplier-information.find-tender.service.gov.uk"
else
  TF_VAR_grafana_url="https://grafana.${ENVIRONMENT}.supplier-information.find-tender.service.gov.uk"
fi
export TF_VAR_grafana_url

export TF_VAR_grafana_token="$(ave aws secretsmanager get-secret-value \
    --secret-id cdp-sirsi-grafana-api-token \
    --query SecretString --output text | jq -r '.API_TOKEN')"

export TF_VAR_teams_webhook_url="$(ave aws secretsmanager get-secret-value \
    --secret-id cdp-sirsi-grafana-alerting-webhook \
    --query SecretString --output text | jq -r '.TEAMS_WEBHOOK_URL')"

export TF_VAR_cloudwatch_datasource_name="CloudWatch"

export TF_VAR_environment="${AWS_ENV:-${TG_ENVIRONMENT:-development}}"
export TF_VAR_cloudwatch_account_id="$(ave aws sts get-caller-identity --query Account --output text)"

ave terraform init -input=false
ave terraform plan -input=false -var-file=env/${TF_VAR_environment}.tfvars
ave terraform apply -input=false -var-file=env/${TF_VAR_environment}.tfvars
```

## Notes

- Alert rules and notification policies can be added incrementally once we decide how to structure them.
- This approach avoids baking Grafana config into the Docker image.

## CI/CD

GitHub Actions workflow: `.github/workflows/grafana-terraform.yml`

- Runs on `main` when files under `terragrunt/tools/grafana/**` change
- Uses OIDC to assume role `cdp-sirsi-terraform` in each account
