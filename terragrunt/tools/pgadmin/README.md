# PG Admin

This configuration is based on the latest published pgAdmin image, to grant dev team access to the DB during development, only in non-prod accounts.

> In the following examples ave is alias for `aws-vault exec` command.
Feel free to use any convenient AWS profiler instead.
## Pin version

```shell
export PINNED_PGADMIN_VERSION=8.12.0
```

## Build

```shell
docker build --build-arg PGADMIN_VERSION=${PINNED_PGADMIN_VERSION} -t cabinetoffice/cdp-pgadmin:${PINNED_PGADMIN_VERSION} .
```

## Deploy

### Push to ECR

There is ECR repositories in orchestrator account. Using the following commands, we can push the built image.

```shell
aws-switch-to-cdp-sirsi-orchestrator-goaco-terraform
ACCOUNT_ID=$(ave aws sts get-caller-identity | jq -r '.Account')
ave aws ecr get-login-password --region eu-west-2 | docker login --username AWS --password-stdin ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com
docker tag cabinetoffice/cdp-pgadmin:${PINNED_PGADMIN_VERSION} ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-pgadmin:${PINNED_PGADMIN_VERSION}
docker push ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-pgadmin:${PINNED_PGADMIN_VERSION}
```

### Re-deploy PG-Admin service

> Note! if the same version is not set for the task's image [here](../../modules/tools/service-pgadmin.tf), you will need to update terraform and provision the service instead of just forcing a new deployment.

```shell
aws-switch-to-cdp-sirsi-development-goaco-terraform
ave aws ecs update-service --cluster cdp-sirsi --service pgadmin --force-new-deployment | jq .
```

## Fetch Credentials

Each account stores a randomly generated password for its Pgadmin instance in a secret called cdp-sirsi-pgadmin-credentials in Secrets Manager. Using the following command, we can retrieve it.

```shell
ave aws secretsmanager get-secret-value --secret-id cdp-sirsi-pgadmin-credentials --query SecretString --output text | jq -r '.'
```
