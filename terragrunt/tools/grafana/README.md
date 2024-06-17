# Grafana

This configuration is based on the latest published Grafana image, includes a CloudWatch data source, and provisions dashboards to enhance observability of SIRSI applications and infrastructure.

```shell
# In the following examples: 
# - ave is alias for `aws-vault exec` command
# - aws-switch-to-* is alias to set the:
#   - AWS_PROFILE
#   - TG_ENVIRONMENT
#   - AWS_ENV (Not compulsory)
#   - MFA_TOKEN handler (Out of scope of this documentation)
# Feel free to use any convenient AWS profiler instead.
```
## build

```shell
docker build -t cdp-grafana .
```

## Push to ECR

There are individual ECR repositories in each account. Using the following commands, we can push the built image to different accounts.

```shell
aws-switch-to-cdp-sirsi-development-goaco-terraform
ACCOUNT_ID=$(ave aws sts get-caller-identity | jq -r '.Account')
ave aws ecr get-login-password --region eu-west-2 | docker login --username AWS --password-stdin ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com
docker tag cdp-grafana:latest ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-grafana:latest
docker push ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-grafana:latest
```

## Fetch Credentials

Each account stores a randomly generated password for its Grafana instance in a secret called cdp-sirsi-grafana-credentials in Secrets Manager. Using the following command, we can retrieve it.

```shell
ave aws secretsmanager get-secret-value --secret-id cdp-sirsi-grafana-credentials --query SecretString --output text | jq -r '.'
```