# Grafana

This configuration is based on the latest published Grafana image, includes a CloudWatch data source, and provisions dashboards and alerts to enhance observability of SIRSI applications and infrastructure.

> In the following examples ave is alias for `aws-vault exec` command.
Feel free to use any convenient AWS profiler instead.

## build

```shell
docker compose build
```

## Provisioning assets

Dashboards and alerting are now provisioned via Terraform under
`/home/abn/Projects/CO/GCGS-Central-Digital-Platform/terragrunt/tools/grafana/terraform/`.
The Docker image only provisions the CloudWatch data source and Grafana config.

## Deploy

### Push to ECR

There is ECR repositories in orchestrator account. Using the following commands, we can push the built image.

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

## Fetch Credentials

Each account stores a randomly generated password for its Grafana instance in a secret called cdp-sirsi-grafana-credentials in Secrets Manager. Using the following command, we can retrieve it.

```shell
ave aws secretsmanager get-secret-value --secret-id cdp-sirsi-grafana-credentials --query SecretString --output text | jq -r '.'
```

## Configure Microsoft Teams alerting

Grafana reads the Teams webhook URL from a Secrets Manager secret named `cdp-sirsi-grafana-alerting-webhook`.
Create it if it doesn't exist and set the webhook URL like this:

```shell
WEBHOOK_URL="https://outlook.office.com/webhook/REPLACE_ME"

aws secretsmanager create-secret \
  --name cdp-sirsi-grafana-alerting-webhook \
  --description "Grafana alerting configuration" \
  --secret-string "{\"TEAMS_WEBHOOK_URL\":\"${WEBHOOK_URL}\"}" \
  || aws secretsmanager put-secret-value \
    --secret-id cdp-sirsi-grafana-alerting-webhook \
    --secret-string "{\"TEAMS_WEBHOOK_URL\":\"${WEBHOOK_URL}\"}"
```

## Configure Grafana API token (for Terraform provisioning)

Grafana Terraform needs an API token stored in Secrets Manager as `cdp-sirsi-grafana-api-token`.
Create it if it doesn't exist and set the token like this:

```shell
API_TOKEN="REPLACE_ME"

aws secretsmanager create-secret \
  --name cdp-sirsi-grafana-api-token \
  --description "Grafana API token for Terraform provisioning" \
  --secret-string "{\"API_TOKEN\":\"${API_TOKEN}\"}" \
  || aws secretsmanager put-secret-value \
    --secret-id cdp-sirsi-grafana-api-token \
    --secret-string "{\"API_TOKEN\":\"${API_TOKEN}\"}"
```
