# ClamAV REST

This configuration is based on the latest published [clamav-rest image](https://hub.docker.com/r/ajilaag/clamav-rest), to allow av-scanner-app scan uploaded files in staging bucket before moving to the live.

> In the following examples ave is alias for `aws-vault exec` command.
Feel free to use any convenient AWS profiler instead.

## Build

```shell
docker build -t cabinetoffice/cdp-clamav-rest:latest .
```

## Deploy

### Push to ECR

There is ECR repositories in orchestrator account. Using the following commands, we can push the built image.

```shell
aws-switch-to-cdp-sirsi-orchestrator-goaco-terraform
ACCOUNT_ID=$(ave aws sts get-caller-identity | jq -r '.Account')
ave aws ecr get-login-password --region eu-west-2 | docker login --username AWS --password-stdin ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com
docker tag cabinetoffice/cdp-clamav-rest:latest ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-clamav-rest:latest
docker push ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-clamav-rest:latest
```

### Re-deploy ClamAV REST service


```shell
aws-switch-to-cdp-sirsi-development-goaco-terraform
ave aws ecs update-service --cluster cdp-sirsi --service clamav-rest --force-new-deployment | jq .
```
