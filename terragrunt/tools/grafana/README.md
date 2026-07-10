# Grafana

Grafana runs as a container in ECS and is configured via Terraform under
`terragrunt/tools/grafana/terraform/`.

Primary docs:
- `terragrunt/docs/grafana.md` (build/deploy + Terraform config)
- `terragrunt/docs/manage-secrets.md` (secrets setup)

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
