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
export TG_ENVIRONMENT=development
ave terragrunt/tools/scripts/grafana-apply.sh
```

To only export TF_VARs without running terraform:

```bash
export TG_ENVIRONMENT=development
source terragrunt/tools/scripts/grafana-env.sh
```

## Notes

- Alert rules and notification policies can be added incrementally once we decide how to structure them.
- This approach avoids baking Grafana config into the Docker image.

## CI/CD

GitHub Actions workflow: `.github/workflows/grafana-*.yml`

- Runs on `main` when files under `terragrunt/tools/grafana/**` change
- Uses OIDC to assume role `cdp-sirsi-terraform` in each account
