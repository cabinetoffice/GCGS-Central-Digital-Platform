# FTS Healthcheck Service

TBC...

> In the following examples ave is alias for `aws-vault exec` command.
Feel free to use any convenient AWS profiler instead.

## build

```shell
docker compose build
```

## run

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
docker tag cabinetoffice/cdp-fts-healthcheck:latest ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-fts-healthcheck:latest
docker push ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-fts-healthcheck:latest
```

### Re-deploy Healthcheck service

```shell
aws-switch-to-cdp-sirsi-development-goaco-terraform
ave aws ecs update-service --cluster cdp-sirsi --service fts-healthcheck --force-new-deployment | jq .
```
