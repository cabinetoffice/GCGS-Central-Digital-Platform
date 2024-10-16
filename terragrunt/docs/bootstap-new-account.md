# Bootstrap a New Account

## Table of Contents
1. [Initiate](#initiate)
2. [Provision Rest of the Components](#provision-rest-of-the-components)
   1. [Create/Update Secrets](#createupdate-secrets)
   2. [Create New Users](#create-new-users)
   3. [Apply Terraform Across All Components](#apply-terraform-across-all-components)
3. [Pin Application/Service Version](#pin-applicationservice-version)

---

## Initiate

**TL;DR:**
```shell
# ave is an alias for the `aws-vault exec` command
# aws-switch-to-* is an alias to set the:
# - AWS_PROFILE
# - TG_ENVIRONMENT
# - AWS_ENV (Not compulsory)
# - MFA_TOKEN handler (Out of scope for this documentation)

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
- Set the `TG_ENVIRONMENT` environment variable (e.g., staging).
- Apply Terraform using Terragrunt while the bootstrap role is assumed.
- When Terragrunt prompts to create the state bucket, allow it.\
  ![bootstrap-start.png](./images/bootstrap-start.png)
- The Core IAM component will create the Terraform role for future use.\
  ![bootstrap-output-terraform-role.png](./images/bootstrap-output-terraform-role.png)
- Assume the Terraform role.
- Confirm the caller identity.\
  ![bootstrap-confirm-terraform-caller.png](./images/bootstrap-confirm-terraform-caller.png)

## Provision Rest of the Components

### Create/Update Secrets

Ensure all required secrets for the target environment are in place by following the [manage-secrets.md](./manage-secrets.md) instructions.

> **Note**: Some secrets belong to the Orchestrator account only.

### Apply Terraform Across All Components
1. Navigate to the components folder.
2. Apply all components while assuming the **corresponding account's Terraform role**.
   ![terragrunt-apply-all](./images/terragrunt-apply-all.png)

### Create New Users

We use Cognito user pools to restrict access to **non-production accounts**. The [cognito_create_user.sh](./tools/scripts/cognito_create_user.sh) script allows us to create new users with randomly generated passwords.

```shell
# To create a user called DP-405
./tools/scripts/cognito_create_user.sh <username>
```

The credentials will also be stored in AWS Secrets Manager under the same account, within the `cdp-sirsi-cognito/users/*` namespace, for future use, such as sharing with third-party users.

## Pin Application/Service Version

To pin services to a specific version in the account, set the `pinned_service_version` in the [main configuration file](../components/terragrunt.hcl). If this value is left null, the system defaults to using the latest published version, as specified in the service-version parameter within the Orchestrator account's SSM.

![pin-service-version](images/pin-service-version.png)
