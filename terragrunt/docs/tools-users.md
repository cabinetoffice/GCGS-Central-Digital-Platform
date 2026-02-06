# Tools User Management (Cognito)

> **Note:** In this documentation, `ave` is an alias for the `aws-vault exec` command.
> You can use any AWS profile/credential helper you prefer.

This guide covers managing users for the **shared Tools Cognito User Pool** used by multiple tools.
The pool name is **`cdp-sirsi-tools`**. Each tool has its own **app client** (for example, s3-uploader),
and access can be restricted via **Cognito groups**.

## Find the User Pool ID

```shell
POOL_ID=$(ave aws cognito-idp list-user-pools --max-results 60 \
  --query 'UserPools[?Name==`cdp-sirsi-tools`].Id' --output text)

echo "POOL_ID=${POOL_ID}"
```

## App clients (per tool)

Each tool should have its own Cognito app client (callback/logout URLs, client ID/secret).
For example, s3-uploader uses a client named `cdp-sirsi-tools-s3-uploader`.
Naming convention: `<tools_pool_name>-<tool>` (for example, `cdp-sirsi-tools-s3-uploader`).

To list clients:

```shell
ave aws cognito-idp list-user-pool-clients \
  --user-pool-id "${POOL_ID}" \
  --max-results 60 \
  --query 'UserPoolClients[].{Name:ClientName,Id:ClientId}' \
  --output table
```

## Create a user (invite flow)

Creates a user and sends an email invite with a temporary password. The user sets their own password on first login.

```shell
EMAIL="user@example.com"

ave aws cognito-idp admin-create-user \
  --user-pool-id "${POOL_ID}" \
  --username "${EMAIL}" \
  --user-attributes Name=email,Value="${EMAIL}" Name=email_verified,Value=true
```

## Groups (recommended access control)

Groups are managed in Terraform. Use the CLI to add users to the appropriate group.

Add a user to a group:
```shell
EMAIL="user@example.com"
GROUP_NAME="tools-s3-uploader"

ave aws cognito-idp admin-add-user-to-group \
  --user-pool-id "${POOL_ID}" \
  --username "${EMAIL}" \
  --group-name "${GROUP_NAME}"
```

List users in a group:

```shell
GROUP_NAME="tools-s3-uploader"

ave aws cognito-idp list-users-in-group \
  --user-pool-id "${POOL_ID}" \
  --group-name "${GROUP_NAME}" \
  --query 'Users[].{Username:Username,Status:UserStatus,Enabled:Enabled}' \
  --output table
```

## Reset a user password

```shell
EMAIL="user@example.com"

ave aws cognito-idp admin-reset-user-password \
  --user-pool-id "${POOL_ID}" \
  --username "${EMAIL}"
```

## Disable / enable a user

```shell
EMAIL="user@example.com"

ave aws cognito-idp admin-disable-user \
  --user-pool-id "${POOL_ID}" \
  --username "${EMAIL}"

ave aws cognito-idp admin-enable-user \
  --user-pool-id "${POOL_ID}" \
  --username "${EMAIL}"
```

## List users

```shell
ave aws cognito-idp list-users \
  --user-pool-id "${POOL_ID}" \
  --query 'Users[].{Username:Username,Status:UserStatus,Enabled:Enabled}' \
  --output table
```

## Self-signup and approvals

The current pool configuration is **admin-create-only** (no public self‑signup). This means:
- Users do **not** choose their own password up front.
- They receive a temporary password via email and set a new password on first login.

If you want **self-signup with admin approval**, Cognito does **not** support that directly; it requires a custom workflow (Lambda triggers or a separate approval system).
