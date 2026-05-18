# Grafana

This doc covers Grafana container deployment.

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
Terraform under [../tools/grafana/terraform](../tools/grafana/terraform)

See the Terraform docs in [../tools/grafana/terraform/README.md](../tools/grafana/terraform/README.md#local-usage).
