# Fluent Bit Log Router (CDP)

This repository builds a custom Fluent Bit container image based on Amazon's official image. It is used in CDP environments to collect, filter, and route logs to CloudWatch or other backends.

It supports:
- Health checks via TCP
- Prometheus `/metrics` endpoint for visibility

---

## Build

```shell
docker compose build
```

If need local testing ...
```shell
 docker compose up
```

## Deploy

### Push to ECR

There is ECR repositories in orchestrator account. Using the following commands, we can push the built image.

```shell
aws-switch-to-cdp-sirsi-orchestrator-goaco-terraform
ACCOUNT_ID=$(ave aws sts get-caller-identity | jq -r '.Account')
ave aws ecr get-login-password --region eu-west-2 | docker login --username AWS --password-stdin ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com
docker tag cabinetoffice/cdp-fluentbit:latest ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-fluentbit:latest
docker push ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-fluentbit:latest
```

### Re-deploy FluentBit service

```shell
aws-switch-to-cdp-sirsi-development-goaco-terraform
ave aws ecs update-service --cluster cdp-sirsi --service fluentbit --force-new-deployment | jq .
```