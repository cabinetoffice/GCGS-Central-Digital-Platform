# Central Digital Platform Infrastructure

This code base is responsible for provisioning the AWS infrastructure needed to support the CDP SIRSI application.

## Table of Contents
1. [Bootstrap New Account](#bootstrap-a-new-account)
2. [Create New User](#create-new-users)
3. [Manage Secrets](#manage-secrets)
   - [Retrieve Diagnostic URI](#Retrieve-iagnostic-uri)
   - [Update Companies House Secrets](#update-companies-house-secrets)
   - [Update FtsService URL](#update-ftsservice-url)
   - [Update GOVUKNotify ApiKey](#update-govuknotify-apikey)
   - [Update GOVUKNotify Support Admin Email](#update-govuknotify-support-admin-email)
   - [Update OneLogin Secrets](#update-onelogin-secrets)
   - [Update Slack Configuration](#update-slack-configuration)
4. [Pin Service Version](#pin-service-version)

## Bootstrap a new account

### Initiate

**TL;DR:**
```shell
# ave is alias for `aws-vault exec` command
# aws-switch-to-* is alias to set the:
# - AWS_PROFILE
# - TG_ENVIRONMENT
# - AWS_ENV (Not compulsory)
# - MFA_TOKEN handler (Out of scope of this documentation)

./tools/delete_tf_cache.sh
cd components/core/iam
aws-switch-to-cdp-sirsi-staging-bootstrap
ave terragrunt apply
aws-switch-to-cdp-sirsi-staging-terraform
ave aws sts get-caller-identity | cat
```

**Summary:**
- Ensure the cache is cleared.
- Navigate to the `core/iam` component.
- Assume the account bootstrap role.
- Set the `TG_ENVIRONMENT` environment variable (staging in the following example).
- Apply Terraform using Terragrunt while the bootstrap role is assumed.
- When Terragrunt prompts for the creation of the state bucket, allow it to be created.\
   ![bootstrap-start.png](../docs/images/infra/bootstrap-start.png)
- Core IAM will create the Terraform role to be used from now on.\
   ![bootstrap-output-terraform-role.png](../docs/images/infra/bootstrap-output-terraform-role.png)
- Assume the Terraform role.
- Confirm the caller identity.\
   ![bootstrao-confirm-terraform-caller.png](../docs/images/infra/bootstrao-confirm-terraform-caller.png)

### Provision rest of the components
- Navigate to the root directory
- Create the OneLogin secret `cdp-sirsi-one-login-credentials`, i.e:
```shell
ave aws secretsmanager create-secret --name cdp-sirsi-one-login-credentials --secret-string '{"Authority":"https://stagingoidc.example.com", "ClientId": "staging-client-id", "PrivateKey":"DEV RSA PRIVATE KEY"}'
```
- Create the Authority secret `cdp-sirsi-authority-key`, i.e:
```shell
make generate-authority-keys
ave make aws-push-authority-keys
```
- Navigate to the root of components
- Apply all, while terraform role is assumed
![terragrunt-apply-all](../docs/images/infra/terragrunt-apply-all.png)

- Build and push images to ECR
```shell
ave make aws-push-to-ecr
```

---

## Create new users

We are using Cognito user pools to restrict access to non-production accounts. The [cognito_create_user.sh](./tools/scripts/cognito_create_user.sh) script allows us to create new users with a randomly generated password.

```shell
# To create a user called DP-405
./tools/scripts/cognito_create_user.sh DP-405
```
The credentials will also be stored in AWS Secrets Manager under the same account, within the cdp-sirsi-cognito/users/* namespace, for future use, such as sharing with third-party users.

---

## Manage Secrets

### Retrieve Diagnostic URI

1. Set your AWS profile to target the specified AWS account, and use the AWS CLI to retrieve the full URL of the diagnostic page for the given account.

```shell
# ave is alias for `aws-vault exec` command
echo "https://$(ave aws route53 list-hosted-zones --query 'HostedZones[0].Name' --output text | sed 's/\.$//')$(ave aws secretsmanager get-secret-value --secret-id cdp-sirsi-diagnostic-path --query 'SecretString' --output text)"
```

### Update Companies House Secrets

1. Create a JSON file in the `./secrets` folder with the following attributes, e.g., **companies-house-secrets-development.json**:

```json
{
    "url": "https://api.company-information.service.gov.uk",
    "User": "<value>",
    "Password": "<value>"
}
```
Note: The `./secrets` folder is set to ignore all files to ensure no sensitive information is committed.

2. Assume the appropriate role for the target environment and update the secret:

```shell
# ave is alias for `aws-vault exec` command
# add using:
# ave aws secretsmanager create-secret --name cdp-sirsi-companies-house-credentials --secret-string file://secrets/companies-house-secrets-development.json | jq .
# or update using:
ave aws secretsmanager put-secret-value --secret-id cdp-sirsi-companies-house-credentials --secret-string file://secrets/companies-house-secrets-development.json | jq .
```
3. Redeploy the `organisation-app` service.

### Update FtsService URL

1. Identify the `FTS service URL` for the specified AWS account.
2. Set your AWS profile to target the specified AWS account, and use the AWS CLI to update the secret.

```shell
# ave is alias for `aws-vault exec` command
# add using:
# ave aws secretsmanager create-secret --name cdp-sirsi-fts-service-url --secret-string "<FTS service URL>" | jq .
# or update using:
ave aws secretsmanager put-secret-value --secret-id cdp-sirsi-fts-service-url --secret-string "<FTS service URL>" | jq .
```

3. Redeploy the `organisation-app` service.

### Update GOVUKNotify ApiKey

1. Identify the `GOV UK Notify API Key` for the specified AWS account.
2. Set your AWS profile to target the specified AWS account, and use the AWS CLI to update the secret.

```shell
# ave is alias for `aws-vault exec` command
# add using:
# ave aws secretsmanager create-secret --name cdp-sirsi-govuknotify-apikey --secret-string "<GOV UK Notify API Key>" | jq .
# or update using:
ave aws secretsmanager put-secret-value --secret-id cdp-sirsi-govuknotify-apikey --secret-string "<GOV UK Notify API Key>" | jq .
```

3. Redeploy the `organisation` service.

### Update GOVUKNotify Support Admin Email

_this is a temporary solution while we are managing such user in the database_
1. Identify the `GOV UK Notify Support Admin Email` for the specified AWS account.
2. Set your AWS profile to target the specified AWS account, and use the AWS CLI to update the secret.

```shell
# ave is alias for `aws-vault exec` command
# add using:
# ave aws secretsmanager create-secret --name cdp-sirsi-govuknotify-support-admin-email --secret-string "<GOV UK Notify API Key>" | jq .
# or update using:
ave aws secretsmanager put-secret-value --secret-id cdp-sirsi-govuknotify-support-admin-email --secret-string "<GOV UK Notify API Key>" | jq .
```

3. Redeploy the `organisation` service.

### Update OneLogin secrets

1. Create a JSON file in the `./secrets` folder with the following attributes, e.g., **onelogin-secrets-development.json**:

```json
{
  "Authority":"https://xxxxx",
  "ClientId": "xxxxx",
  "PrivateKey":"-----BEGIN RSA PRIVATE KEY-----\nxxxx\nxxxx==\n-----END RSA PRIVATE KEY-----%"
}
```
Note: The `./secrets` folder is set to ignore all files to ensure no sensitive information is committed.

2. Assume the appropriate role for the target environment and update the secret:

```shell
# ave is alias for `aws-vault exec` command
# add using:
# ave aws secretsmanager create-secret --name cdp-sirsi-one-login-credentials --secret-string file://secrets/onelogin-secrets-development.json | jq .
# or update using:
ave aws secretsmanager put-secret-value --secret-id cdp-sirsi-one-login-credentials --secret-string file://secrets/onelogin-secrets-development.json | jq .
```
3. Redeploy the `organisation-app` service.

### Update Slack Configuration

When the orchestrator's notification component is enabled, the system will notify a specified Slack channel about important CI/CD events. The required configuration for this connection must be stored as a secret named slack-configuration in the Orchestrator account. To create this secret, add a file named slack-notification-api-endpoint.txt under the secrets directory, containing a single line with the Slack API endpoint. Then, run the following command.

```shell
aws-switch-to-cdp-sirsi-orchestrator-goaco-terraform
ave aws secretsmanager create-secret --name cdp-sirsi-slack-api-endpoint --name cdp-sirsi-fts-service-url --secret-string file://secrets/slack-notification-api-endpoint.txt | jq .

```

This command will create a secret named cdp-sirsi-slack-api-endpoint in AWS Secrets Manager, setting its value from the contents of the slack-notification-api-endpoint.txt file in the secrets' directory.

---

## Pin Service Version

To pin services to a specific version in the given account, we can set the pinned_service_version in the [main configuration file](./components/terragrunt.hcl). If this value is left null, the system will default to using the latest published version, as specified in the service-version parameter within the Orchestrator account's SSM.

![pin-service-version](../docs/images/infra/pin-service-version.png)
