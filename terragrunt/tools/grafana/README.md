# Grafana

This configuration is based on the latest published Grafana image, includes a CloudWatch data source, and provisions dashboards to enhance observability of SIRSI applications and infrastructure.

> In the following examples ave is alias for `aws-vault exec` command.
Feel free to use any convenient AWS profiler instead.

## build

```shell
docker build -t cabinetoffice/cdp-grafana .
```

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