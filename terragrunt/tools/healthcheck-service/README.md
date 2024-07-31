# Healthcheck Service

TBC...

> In the following examples ave is alias for `aws-vault exec` command.
Feel free to use any convenient AWS profiler instead.

## build

```shell
docker build -t cabinetoffice/cdp-healthcheck .
```

## run

```shell
docker run -d -p 3030:3030 -e ASPNETCORE_URLS=http://+:3030 -e ASPNETCORE_PORT=3030 -e QUEUE_URL_ORGANISATION="TQ_Organisation" -e QUEUE_URL_ENTITY_VERIFICATION="TQ_EntityVerification" cabinetoffice/cdp-healthcheck

```

## Deploy

### Push to ECR

There is ECR repositories in orchestrator account. Using the following commands, we can push the built image.

```shell
aws-switch-to-cdp-sirsi-orchestrator-goaco-terraform
ACCOUNT_ID=$(ave aws sts get-caller-identity | jq -r '.Account')
ave aws ecr get-login-password --region eu-west-2 | docker login --username AWS --password-stdin ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com
docker tag cabinetoffice/cdp-healthcheck:latest ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-healthcheck:latest
docker push ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-healthcheck:latest
```

### Re-deploy Healthcheck service

```shell
aws-switch-to-cdp-sirsi-development-goaco-terraform
ave aws ecs update-service --cluster cdp-sirsi --service healthcheck --force-new-deployment | jq .
```

```shell
aws-switch-to-cdp-sirsi-staging-goaco-terraform
ave aws ecs update-service --cluster cdp-sirsi --service healthcheck --force-new-deployment | jq .
```

```shell
aws-switch-to-cdp-sirsi-integration-goaco-terraform
ave aws ecs update-service --cluster cdp-sirsi --service healthcheck --force-new-deployment | jq .
```
