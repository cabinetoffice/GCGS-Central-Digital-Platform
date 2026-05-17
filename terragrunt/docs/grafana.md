# Grafana

This doc covers Grafana container deployment and Terraform-based configuration.

> In these examples, `ave` is an alias for `aws-vault exec`. You can use any
> AWS profile manager that sets the same environment variables.

## Build and deploy the container

```shell
docker compose build
```

### Push to ECR (orchestrator account)

```shell
aws-switch-to-cdp-sirsi-orchestrator-goaco-terraform
ACCOUNT_ID=$(ave aws sts get-caller-identity | jq -r '.Account')
ave aws ecr get-login-password --region eu-west-2 | docker login --username AWS --password-stdin ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com
docker tag cabinetoffice/cdp-grafana:latest ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-grafana:latest
docker push ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-grafana:latest
```

### Re-deploy Grafana service

```shell
ave aws ecs update-service --cluster cdp-sirsi --service grafana --force-new-deployment | jq -r '.'
```

## Terraform configuration

Grafana config (dashboards, alerts, contact points, data sources) is managed by
Terraform under:

```
terragrunt/tools/grafana/terraform
```

### Local usage

```shell
ACCOUNT_ID="$(ave aws sts get-caller-identity --query Account --output text)"
GRAFANA_API_TOKEN="$(ave aws secretsmanager get-secret-value \
  --secret-id cdp-sirsi-grafana-api-token \
  --query SecretString --output text | jq -r '.API_TOKEN')"
GRAFANA_TEAMS_WEBHOOK_URL="$(ave aws secretsmanager get-secret-value \
  --secret-id cdp-sirsi-grafana-alerting-webhook \
  --query SecretString --output text 2>/dev/null | jq -r '.TEAMS_WEBHOOK_URL')"

if [ "${TG_ENVIRONMENT:-development}" = "production" ]; then
  export TF_VAR_grafana_url="https://grafana.supplier-information.find-tender.service.gov.uk"
elif [ "${TG_ENVIRONMENT:-development}" = "development" ]; then
  export TF_VAR_grafana_url="https://grafana.dev.supplier-information.find-tender.service.gov.uk"
else
  export TF_VAR_grafana_url="https://grafana.${TG_ENVIRONMENT:-development}.supplier-information.find-tender.service.gov.uk"
fi

export TF_VAR_grafana_token="${GRAFANA_API_TOKEN}"
export TF_VAR_teams_webhook_url="${GRAFANA_TEAMS_WEBHOOK_URL:-}"
export TF_VAR_environment="${TG_ENVIRONMENT:-development}"
export TF_VAR_cloudwatch_account_id="${ACCOUNT_ID}"
export TF_VAR_cloudwatch_assume_role_arn="arn:aws:iam::${ACCOUNT_ID}:role/cdp-sirsi-telemetry"

cat > backend.hcl <<EOF
bucket = "tfstate-cdp-sirsi-${TG_ENVIRONMENT:-development}-${ACCOUNT_ID}"
key    = "tools/grafana/terraform/terraform.tfstate"
region = "eu-west-2"
encrypt = true
use_lockfile = true
EOF

terraform init -input=false -reconfigure \
  -backend-config=backend.hcl

terraform plan -input=false -var-file=env/${TG_ENVIRONMENT:-development}.tfvars
terraform apply -input=false -var-file=env/${TG_ENVIRONMENT:-development}.tfvars
```

### Dashboards

Place dashboard JSON files under:

```
terragrunt/tools/grafana/terraform/dashboards/
```

Folders map to subdirectories:
- `application`
- `infrastructure`
- `overview`
- `traffic`

Dashboard UIDs are deterministic from folder + filename to keep URLs stable.

## Secrets

Secret creation/updates are documented in:
`terragrunt/docs/manage-secrets.md`.
