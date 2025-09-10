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
- [Update Charity Commission Secrets](#update-charity-commission-secrets)
- [Update Companies House Secrets](#update-companies-house-secrets)
- [Update FtsService URL](#update-ftsservice-url)
- [Update GOVUKNotify ApiKey](#update-govuknotify-apikey)
- [Update GOVUKNotify Support Admin Email](#update-govuknotify-support-admin-email)
- [Update ODI Data Platform Secrets](#update-odi-data-platform-secret)
- [Update OneLogin Forward Logout Notification API Key](#update-onelogin-forward-logout-notification-api-key)
- [Update OneLogin Secrets](#update-onelogin-secrets)
- [Update Pen Testing Configuration](#update-pen-testing-configuration)
- [Update Production Database Users](#update-production-database-users)
- [Update Slack Configuration](#update-slack-configuration)
- [Update Terraform Operators](#update-terraform-operators)
- [Update WAF Allowed IP Set](#update-waf-allowed-ip-set)

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

## Update Charity Commission Secrets

1. Create a JSON file in the `./secrets` folder with the following attributes, e.g., **charity-commission-api.json**:

```json
{
  "Url": "https://api.charityxxx/xxx/xxx",
  "SubscriptionKey": "xxxxxxxxxxxxxxxxxx"
}
```
*Note: The `./secrets` folder is set to ignore all files to ensure no sensitive information is committed.*

2. Assume the appropriate role for the target environment and update the secret:

```shell
# Add using:
# ave aws secretsmanager create-secret --name cdp-sirsi-charity-commission-credentials --secret-string file://secrets/charity-commission-api.json | jq .
# Or update using:
ave aws secretsmanager put-secret-value --secret-id cdp-sirsi-charity-commission-credentials --secret-string file://secrets/charity-commission-api.json | jq .
```

3. Redeploy the `organisation-app` service.

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

## Update ODI Data Platform Secrets

1. Create a JSON file in the `./secrets` folder with the following attributes, e.g., **odi-data-platform-development.json**:

```json
{
    "Url": "",
    "ApiKey": ""
}
```
*Note: The `./secrets` folder is set to ignore all files to ensure no sensitive information is committed.*

2. Assume the appropriate role for the target environment and update the secret:

```shell
aws-vault exec cdp-sirsi-development-terraform -- aws secretsmanager create-secret --name cdp-sirsi-odi-data-platform-secret --secret-string file://secrets/odi-data-platform-secret.json | jq .
# Add using:
# ave aws secretsmanager create-secret --name cdp-sirsi-odi-data-platform --secret-string file://secrets/odi-data-platform-development.json | jq .
# Or update using:
ave aws secretsmanager put-secret-value --secret-id cdp-sirsi-odi-data-platform --secret-string file://secrets/odi-data-platform-development.json | jq .
```

3. Redeploy the `service-commercial-tools-app` and 'service-commercial-tools-api' services.

---

## Update OneLogin Secrets

1. Create a JSON file in the `./secrets` folder with the following attributes, e.g., **onelogin-secrets-development.json**:

```json
{
  "AccountUrl": "https://xxxx",
  "Authority": "https://xxxxx",
  "ClientId": "xxxxx",
  "PrivateKey": "-----BEGIN RSA PRIVATE KEY-----\nxxxx\nxxxx==\n-----END RSA PRIVATE KEY-----"
}
```
> The Authority should only contain the scheme and domain name (e.g., https://example.com). The application will automatically append the necessary URI (e.g., /.well-known/openid-configuration), so you don't need to include it in this field.

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

## Update OneLogin Forward Logout Notification API Key

1. Use `uuidgen` or a similar tool to generate a new API key and store it in the target environment's Secrets Manager.

```shell
# Assume the appropriate role for the target environment and...
# Add a new API key:
# ave aws secretsmanager create-secret --name cdp-sirsi-one-login-forward-logout-notification-api-key --secret-string $(uuidgen) | jq .

# Or update an existing API key:
ave aws secretsmanager put-secret-value --secret-id cdp-sirsi-one-login-forward-logout-notification-api-key --secret-string $(uuidgen) | jq .

```

2. Retrieve the stored API key from Secrets Manager and share it securely with the relevant teams using a secure medium (e.g., encrypted email, password manager).

```shell
ave aws secretsmanager get-secret-value --secret-id cdp-sirsi-one-login-forward-logout-notification-api-key | jq .SecretString

```

3. Redeploy the `organisation-app` service.

---

## Update Pen Testing Configuration

1. Create a JSON file in the `./secrets` folder with the list of operators' IAM user ARNs who will be granted permission to assume the Terraform role in the account, e.g., **pen-testing-configuration.json**:

```json
{
  "allowed_ips": [
    "123.123.123.123",
    "321.321.21.0/24"
  ],
  "user_arns": [
    "arn:aws:iam::123456789999:user/user1",
    "arn:aws:iam::123456789999:user/user2"
  ],
  "external_user_arns": []
}
```

*Note: The `./secrets` folder is set to ignore all files to ensure no sensitive information is committed.*

2. Assume the appropriate role for the target environment and update the secret:

```shell
# Add using:
# ave aws secretsmanager create-secret --name cdp-sirsi-pen-testing-configuration --secret-string file://secrets/pen-testing-configuration.json | jq .
# Or update using:
ave aws secretsmanager put-secret-value --secret-id cdp-sirsi-pen-testing-configuration --secret-string file://secrets/pen-testing-configuration.json | jq .
```

3. Plan and apply Terraform to the `core/iam` component.

---

## Update Production Database Users

> Note:\
> This is a temporary solution until the application has the capability to manage data updates as required.\
> Ensure you do not accidentally remove previously approved users. Always check the existing list in the cdp-sirsi-pgadmin-production-support-users secret before making updates.

1. Identify the list of users who need full permissions over production-level data.
2. prepare a comma separted list of those usernames i.e. `firstname1.lastname1,firstname2.lastname2`
3. Set your AWS profile to target the Production's AWS account, and use the AWS CLI to update the secret:

```shell
aws-switch-to-cdp-sirsi-production-goaco-terraform
# Add using:
# ave aws secretsmanager create-secret --name cdp-sirsi-pgadmin-production-support-users --secret-string "firstname1.lastname1,firstname2.lastname2" | jq .
# Or update using:
ave aws secretsmanager put-secret-value --secret-id cdp-sirsi-pgadmin-production-support-users --secret-string "firstname1.lastname1,firstname2.lastname2,firstname3.lastname3" | jq .
```

4. Redeploy the `pgadmin` service.
5. Refer to the [../tools/pgadmin](../tools/pgadmin) for instructions on creating database users with matching IDs and granting them the necessary privileges.

---

## Update Slack Configuration

When the orchestrator's notification component is enabled, the system will notify a specified Slack channel about important CI/CD events. The required configuration for this connection must be stored as a secret.

1.  Add a file named `cdp-sirsi-slack-configuration.json` under the `secrets` directory, containing the following:

```json
{
  "API_ENDPOINT": "https://slack.com/api",
  "API_AUTH": "Bearer xxx-xxxxxxxxx-xxxxxx-xxxx",
  "SERVICE_ENDPOINT": "https://hooks.slack.com/services/xxxx/xxxxx/xxxx" 
}
```
*Note: The `./secrets` folder is set to ignore all files to ensure no sensitive information is committed.*

2. Assume the appropriate role for the orchestrator environment and update the secret:

```shell
aws-switch-to-cdp-sirsi-orchestrator-goaco-terraform
# Add using:
# ave aws secretsmanager create-secret --name cdp-sirsi-slack-configuration --secret-string file://secrets/cdp-sirsi-slack-configuration.json | jq .
# Or update using:
ave aws secretsmanager put-secret-value --secret-id cdp-sirsi-slack-configuration --secret-string file://secrets/cdp-sirsi-slack-configuration.json | jq .
```

---

## Update Terraform Operators

1. Create a JSON file in the `./secrets` folder with the list of operators' IAM user ARNs who will be granted permission to assume the Terraform role in the account, e.g., **terraform-operators.json**:

```json
{
  "operators": [
    "arn:aws:iam::123456789999:user/user1",
    "arn:aws:iam::123456789999:user/user2"
  ]
}
```

*Note: The `./secrets` folder is set to ignore all files to ensure no sensitive information is committed.*

2. Assume the appropriate role for the target environment and update the secret:

```shell
# Add using:
# ave aws secretsmanager create-secret --name cdp-sirsi-terraform-operators --secret-string file://secrets/terraform-operators.json | jq .
# Or update using:
ave aws secretsmanager put-secret-value --secret-id cdp-sirsi-terraform-operators --secret-string file://secrets/terraform-operators.json | jq .
```

3. Plan and apply Terraform to the `core/iam` component.

---

## Update WAF Allowed IP Set

1. Create a JSON file in the `./secrets` folder containing the list of IP addresses/ranges with comments to identify the owners. Name the file based on the target environment, for example:, e.g., **waf-allowed-ip-set-development.json**, **waf-allowed-ip-set-development-tools.json**:
```json
[
  { "value": "123.123.123.123/32", "comment": "User X in Y team" },
  { "value": "54.54.54.0/16", "comment": "Team Y, X component's IP Range" }
]


```

*Note: The `./secrets` folder is set to ignore all files to ensure no sensitive information is committed.*

2. Assume the appropriate role for the target environment and update the secret:

```shell
# Add for services using:
# ave aws secretsmanager create-secret --name cdp-sirsi-waf-allowed-ip-set --secret-string file://secrets/waf-allowed-ip-set-development.json | jq .
# OR for tools
# ave aws secretsmanager create-secret --name cdp-sirsi-tools-waf-allowed-ip-set --secret-string file://secrets/waf-allowed-ip-set-development-tools.json | jq .
cdp-sirsi-tools-waf-allowed-ip-set
# Or update for services using:
ave aws secretsmanager put-secret-value --secret-id cdp-sirsi-waf-allowed-ip-set --secret-string file://secrets/waf-allowed-ip-set-development.json | jq .
# OR for tools
# ave aws secretsmanager put-secret-value --secret-id cdp-sirsi-tools-waf-allowed-ip-set --secret-string file://secrets/waf-allowed-ip-set-development-tools.json | jq .
```

3. Plan and apply Terraform to the `core/networking` component.

---
