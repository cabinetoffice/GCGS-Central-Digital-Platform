# CLOUD Beaver (WIP)

This configuration is based on the latest published cloud beaver image, to grant dev team access to the DB during development, only in non-prod accounts.

> In the following examples ave is alias for `aws-vault exec` command.
Feel free to use any convenient AWS profiler instead.
## Pin version

## Build

```shell
docker build -t cabinetoffice/cdp-cloud-beaver .
```

If need local testing ...
```shell
 docker run cabinetoffice/cdp-cloud-beaver:latest
```

## Deploy

### Push to ECR

There is ECR repositories in orchestrator account. Using the following commands, we can push the built image.

```shell
aws-switch-to-cdp-sirsi-orchestrator-goaco-terraform
ACCOUNT_ID=$(ave aws sts get-caller-identity | jq -r '.Account')
ave aws ecr get-login-password --region eu-west-2 | docker login --username AWS --password-stdin ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com
docker tag cabinetoffice/cdp-cloud-beaver:latest ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-cloud-beaver:latest
docker push ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-cloud-beaver:latest
```

### Re-deploy Cloud Beaver service

```shell
aws-switch-to-cdp-sirsi-development-goaco-terraform
ave aws ecs update-service --cluster cdp-sirsi --service cloud-beaver --force-new-deployment | jq .
```

## Fetch Credentials

Each account stores a randomly generated password for its Cloud-Beaver instance in a secret called cdp-sirsi-cloud-beaver-credentials in Secrets Manager. Using the following command, we can retrieve it.

```shell
ave aws secretsmanager get-secret-value --secret-id cdp-sirsi-cloud-beaver-credentials --query SecretString --output text | jq -r '.'
```

