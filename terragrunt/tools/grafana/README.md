# Grafana

Grafana runs as a container in ECS and is configured via Terraform under
`terragrunt/tools/grafana/terraform/`.

Primary docs:
- `terragrunt/docs/grafana.md` (build/deploy + Terraform config)
- `terragrunt/docs/manage-secrets.md` (secrets setup)

## Build + push image

From `terragrunt/tools/grafana/`:

```sh
docker compose build
```

Push `:latest` to ECR (orchestrator account):

```sh
aws-switch-to-cdp-sirsi-orchestrator-goaco-terraform
ACCOUNT_ID=$(ave aws sts get-caller-identity | jq -r '.Account')
ave aws ecr get-login-password --region eu-west-2 | docker login --username AWS --password-stdin ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com
docker tag cabinetoffice/cdp-grafana:latest ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-grafana:latest
docker push ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-grafana:latest
```

Then force a re-deploy in the target environment account:

```sh
ave aws ecs update-service --cluster cdp-sirsi --service grafana --force-new-deployment | jq -r '.'
```

## Admin credentials secret

Grafana reads its admin credentials from AWS Secrets Manager secret
`cdp-sirsi-grafana-credentials` (managed/rotated manually per environment).

## Entra ID SSO secret

Grafana SSO expects an AWS Secrets Manager secret named:
`cdp-sirsi-grafana-entra-id`

Create a JSON file in `./secrets` (gitignored), for example
`grafana-entra-id.json`:

```json
{
  "CLIENT_ID": "<entra app client id>",
  "CLIENT_SECRET": "<entra app client secret>",
  "AUTH_URL": "https://login.microsoftonline.com/<tenant-id>/oauth2/v2.0/authorize",
  "TOKEN_URL": "https://login.microsoftonline.com/<tenant-id>/oauth2/v2.0/token"
}
```

Then create or update the secret (per environment account):

```shell
# Create (first time)
ave aws secretsmanager create-secret \
  --name cdp-sirsi-grafana-entra-id \
  --description "Grafana Entra ID OAuth config" \
  --secret-string file://secrets/grafana-entra-id.json \
  || \
# Update (existing)
ave aws secretsmanager put-secret-value \
  --secret-id cdp-sirsi-grafana-entra-id \
  --secret-string file://secrets/grafana-entra-id.json
```
