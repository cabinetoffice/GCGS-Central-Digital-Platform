# Grafana Terraform (configuration)

This folder provisions Grafana configuration via the Grafana API (dashboards and alert contact points).
It is intentionally separate from the core infra Terraform so changes can be applied independently.

## Prerequisites

- A Grafana **API token** per environment (stored in Secrets Manager):
  - Secret name (per account): `cdp-sirsi-grafana-api-token`
  - Secret JSON keys:
    - `GRAFANA_URL` (e.g. `https://grafana.<public_domain>`)
    - `GRAFANA_TOKEN`

- Teams webhook (optional):
  - Secret name: `cdp-sirsi-grafana-alerting`
  - Secret JSON key:
    - `TEAMS_WEBHOOK_URL`

## Dashboard provisioning

Place Grafana dashboard JSON files under `dashboards/`.
Each `*.json` file is provisioned as a dashboard.

## Local usage

```bash
export TF_VAR_grafana_url="$(aws secretsmanager get-secret-value --secret-id cdp-sirsi-grafana-api-token --query SecretString --output text | jq -r '.GRAFANA_URL')"
export TF_VAR_grafana_token="$(aws secretsmanager get-secret-value --secret-id cdp-sirsi-grafana-api-token --query SecretString --output text | jq -r '.GRAFANA_TOKEN')"
export TF_VAR_teams_webhook_url="$(aws secretsmanager get-secret-value --secret-id cdp-sirsi-grafana-alerting --query SecretString --output text | jq -r '.TEAMS_WEBHOOK_URL')"

terraform init
terraform plan
terraform apply
```

## Notes

- Alert rules and notification policies can be added incrementally once we decide how to structure them.
- This approach avoids baking Grafana config into the Docker image.

## CI/CD

GitHub Actions workflow: `.github/workflows/grafana-terraform.yml`

- Runs on `main` when files under `terragrunt/tools/grafana/**` change
- Uses OIDC to assume role `cdp-sirsi-terraform` in each account
