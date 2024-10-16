# Manage Secrets

> **Note:** In this documentation, `ave` is an alias for the `aws-vault exec` command, and `aws-switch-to-*` is an alias that configures the following:
> - `AWS_PROFILE`
> - `TG_ENVIRONMENT`
> - `AWS_ENV` (optional)
> - `MFA_TOKEN` handler (out of scope for this documentation)
>
> You are welcome to use any profile manager or tool you are more comfortable with.

## Table of Contents
- [Retrieve Diagnostic URI](#retrieve-diagnostic-uri)
- [Update Authority Secrets](#update-authority-secrets)
- [Update Companies House Secrets](#update-companies-house-secrets)
- [Update FtsService URL](#update-ftsservice-url)
- [Update GOVUKNotify ApiKey](#update-govuknotify-apikey)
- [Update GOVUKNotify Support Admin Email](#update-govuknotify-support-admin-email)
- [Update OneLogin Secrets](#update-onelogin-secrets)
- [Update Slack Configuration](#update-slack-configuration)

---

## Retrieve Diagnostic URI

1. Set your AWS profile to target the specified AWS account, and use the AWS CLI to retrieve the full URL of the diagnostic page for the given account.

```shell

echo "https://$(ave aws route53 list-hosted-zones --query 'HostedZones[0].Name' --output text | sed 's/\.$//')$(ave aws secretsmanager get-secret-value --secret-id cdp-sirsi-diagnostic-path --query 'SecretString' --output text)"
```

---

## Update Authority Secrets

1. Execute the following commands from the repository's root:
```shell
make generate-authority-keys
ave make aws-push-authority-private-key
```

---

## Update Companies House Secrets

1. Create a JSON file in the `./secrets` folder with the following attributes, e.g., **companies-house-secrets-development.json**:

```json
{
    "url": "https://api.company-information.service.gov.uk",
    "User": "<value>",
    "Password": "<value>"
}
```
*Note: The `./secrets` folder is set to ignore all files to ensure no sensitive information is committed.*

2. Assume the appropriate role for the target environment and update the secret:

```shell
# Add using:
# ave aws secretsmanager create-secret --name cdp-sirsi-companies-house-credentials --secret-string file://secrets/companies-house-secrets-development.json | jq .
# Or update using:
ave aws secretsmanager put-secret-value --secret-id cdp-sirsi-companies-house-credentials --secret-string file://secrets/companies-house-secrets-development.json | jq .
```

3. Redeploy the `organisation-app` service.

---

## Update FtsService URL

1. Identify the `FTS service URL` for the specified AWS account.
2. Set your AWS profile to target the specified AWS account, and use the AWS CLI to update the secret:

```shell
# Add using:
# ave aws secretsmanager create-secret --name cdp-sirsi-fts-service-url --secret-string "<FTS service URL>" | jq .
# Or update using:
ave aws secretsmanager put-secret-value --secret-id cdp-sirsi-fts-service-url --secret-string "<FTS service URL>" | jq .
```

3. Redeploy the `organisation-app` service.

---

## Update GOVUKNotify ApiKey

1. Identify the `GOV UK Notify API Key` for the specified AWS account.
2. Set your AWS profile to target the specified AWS account, and use the AWS CLI to update the secret:

```shell
# Add using:
# ave aws secretsmanager create-secret --name cdp-sirsi-govuknotify-apikey --secret-string "<GOV UK Notify API Key>" | jq .
# Or update using:
ave aws secretsmanager put-secret-value --secret-id cdp-sirsi-govuknotify-apikey --secret-string "<GOV UK Notify API Key>" | jq .
```

3. Redeploy the `organisation` service.

---

## Update GOVUKNotify Support Admin Email

*This is a temporary solution while we are managing such users in the database.*

1. Identify the `GOV UK Notify Support Admin Email` for the specified AWS account.
2. Set your AWS profile to target the specified AWS account, and use the AWS CLI to update the secret:

```shell
# Add using:
# ave aws secretsmanager create-secret --name cdp-sirsi-govuknotify-support-admin-email --secret-string "<GOV UK Notify Support Admin Email>" | jq .
# Or update using:
ave aws secretsmanager put-secret-value --secret-id cdp-sirsi-govuknotify-support-admin-email --secret-string "<GOV UK Notify Support Admin Email>" | jq .
```

3. Redeploy the `organisation` service.

---

## Update OneLogin Secrets

1. Create a JSON file in the `./secrets` folder with the following attributes, e.g., **onelogin-secrets-development.json**:

```json
{
  "Authority": "https://xxxxx",
  "ClientId": "xxxxx",
  "PrivateKey": "-----BEGIN RSA PRIVATE KEY-----\nxxxx\nxxxx==\n-----END RSA PRIVATE KEY-----"
}
```
*Note: The `./secrets` folder is set to ignore all files to ensure no sensitive information is committed.*

2. Assume the appropriate role for the target environment and update the secret:

```shell
# Add using:
# ave aws secretsmanager create-secret --name cdp-sirsi-one-login-credentials --secret-string file://secrets/onelogin-secrets-development.json | jq .
# Or update using:
ave aws secretsmanager put-secret-value --secret-id cdp-sirsi-one-login-credentials --secret-string file://secrets/onelogin-secrets-development.json | jq .
```

3. Redeploy the `organisation-app` service.

---

## Update Slack Configuration

When the orchestrator's notification component is enabled, the system will notify a specified Slack channel about important CI/CD events. The required configuration for this connection must be stored as a secret named `slack-configuration` in the Orchestrator account. To create this secret, add a file named `slack-notification-api-endpoint.txt` under the secrets directory, containing a single line with the Slack API endpoint. Then, run the following command:

```shell
aws-switch-to-cdp-sirsi-orchestrator-goaco-terraform
ave aws secretsmanager create-secret --name cdp-sirsi-slack-api-endpoint --secret-string file://secrets/slack-notification-api-endpoint.txt | jq .
```

This command will create a secret named `cdp-sirsi-slack-api-endpoint` in AWS Secrets Manager, setting its value from the contents of the `slack-notification-api-endpoint.txt` file in the secrets directory.
