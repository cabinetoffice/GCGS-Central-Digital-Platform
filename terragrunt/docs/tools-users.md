# Tools User Management (Cognito)

> **Note:** In this documentation, `ave` is an alias for the `aws-vault exec` command.
> You can use any AWS profile/credential helper you prefer.

This guide covers managing users for the **shared Tools Cognito User Pool** used by multiple tools,
and the **separate OpenSearch user pools** for admin/gateway/debug access.

Pools:
- **Shared tools pool:** `cdp-sirsi-tools`
- **OpenSearch admin pool:** `cdp-sirsi-opensearch-admin` (domain prefix: `cdp-sirsi-<env>-opensearch-admin`)
- **OpenSearch gateway pool:** `cdp-sirsi-opensearch-gateway` (domain prefix: `cdp-sirsi-<env>-opensearch-gateway`)
- **OpenSearch debugtask pool:** `cdp-sirsi-opensearch-debugtask` (domain prefix: `cdp-sirsi-<env>-opensearch-debugtask`)

Each tool/service has its own **app client** (for example, s3-uploader, opensearch-admin),
and access can be restricted via **Cognito groups** (only configured for the shared tools pool today).

## Find the User Pool ID

### Shared tools pool

```shell
POOL_ID=$(ave aws cognito-idp list-user-pools --max-results 60 \
  --query 'UserPools[?Name==`cdp-sirsi-tools`].Id' --output text)

echo "POOL_ID=${POOL_ID}"
```

### OpenSearch admin pool

```shell
OPENSEARCH_ADMIN_POOL_ID=$(ave aws cognito-idp list-user-pools --max-results 60 \
  --query 'UserPools[?Name==`cdp-sirsi-opensearch-admin`].Id' --output text)

echo "OPENSEARCH_ADMIN_POOL_ID=${OPENSEARCH_ADMIN_POOL_ID}"
```

### OpenSearch gateway pool

```shell
OPENSEARCH_GATEWAY_POOL_ID=$(ave aws cognito-idp list-user-pools --max-results 60 \
  --query 'UserPools[?Name==`cdp-sirsi-opensearch-gateway`].Id' --output text)

echo "OPENSEARCH_GATEWAY_POOL_ID=${OPENSEARCH_GATEWAY_POOL_ID}"
```

### OpenSearch debugtask pool

```shell
OPENSEARCH_DEBUGTASK_POOL_ID=$(ave aws cognito-idp list-user-pools --max-results 60 \
  --query 'UserPools[?Name==`cdp-sirsi-opensearch-debugtask`].Id' --output text)

echo "OPENSEARCH_DEBUGTASK_POOL_ID=${OPENSEARCH_DEBUGTASK_POOL_ID}"
```

## App clients (per tool)

Each tool should have its own Cognito app client (callback/logout URLs, client ID/secret).
Examples:
- Shared tools: `cdp-sirsi-tools-s3-uploader`
- OpenSearch admin: `cdp-sirsi-opensearch-admin`
- OpenSearch gateway: `cdp-sirsi-opensearch-gateway`
- OpenSearch debugtask: `cdp-sirsi-opensearch-debugtask`

To list clients:

```shell
ave aws cognito-idp list-user-pool-clients \
  --user-pool-id "${POOL_ID}" \
  --max-results 60 \
  --query 'UserPoolClients[].{Name:ClientName,Id:ClientId}' \
  --output table | cat
```

```shell
ave aws cognito-idp list-user-pool-clients \
  --user-pool-id "${OPENSEARCH_ADMIN_POOL_ID}" \
  --max-results 60 \
  --query 'UserPoolClients[].{Name:ClientName,Id:ClientId}' \
  --output table | cat
```

```shell
ave aws cognito-idp list-user-pool-clients \
  --user-pool-id "${OPENSEARCH_GATEWAY_POOL_ID}" \
  --max-results 60 \
  --query 'UserPoolClients[].{Name:ClientName,Id:ClientId}' \
  --output table | cat
```

```shell
ave aws cognito-idp list-user-pool-clients \
  --user-pool-id "${OPENSEARCH_DEBUGTASK_POOL_ID}" \
  --max-results 60 \
  --query 'UserPoolClients[].{Name:ClientName,Id:ClientId}' \
  --output table | cat
```

## Create a user (invite flow)

Creates a user and sends an email invite with a temporary password. The user sets their own password on first login.

### Shared tools pool

```shell
POOL_ID=$(ave aws cognito-idp list-user-pools --max-results 60 \
  --query 'UserPools[?Name==`cdp-sirsi-tools`].Id' --output text)
echo "POOL_ID=${POOL_ID}"

EMAIL="ali.bahman@goaco.com"

# Shared tools pool
ave aws cognito-idp admin-create-user \
  --user-pool-id "${POOL_ID}" \
  --username "${EMAIL}" \
  --user-attributes Name=email,Value="${EMAIL}" Name=email_verified,Value=true
```

### OpenSearch admin pool

```shell
OPENSEARCH_ADMIN_POOL_ID=$(ave aws cognito-idp list-user-pools --max-results 60 \
  --query 'UserPools[?Name==`cdp-sirsi-opensearch-admin`].Id' --output text)
echo "OPENSEARCH_ADMIN_POOL_ID=${OPENSEARCH_ADMIN_POOL_ID}"

EMAIL="ali.bahman@goaco.com"

# OpenSearch admin pool
ave aws cognito-idp admin-create-user \
  --user-pool-id "${OPENSEARCH_ADMIN_POOL_ID}" \
  --username "${EMAIL}" \
  --user-attributes Name=email,Value="${EMAIL}" Name=email_verified,Value=true | jq .
```

### OpenSearch gateway pool

```shell
OPENSEARCH_GATEWAY_POOL_ID=$(ave aws cognito-idp list-user-pools --max-results 60 \
  --query 'UserPools[?Name==`cdp-sirsi-opensearch-gateway`].Id' --output text)
echo "OPENSEARCH_GATEWAY_POOL_ID=${OPENSEARCH_GATEWAY_POOL_ID}"

EMAIL="ali.bahman@goaco.com"

# OpenSearch gateway pool
ave aws cognito-idp admin-create-user \
  --user-pool-id "${OPENSEARCH_GATEWAY_POOL_ID}" \
  --username "${EMAIL}" \
  --user-attributes Name=email,Value="${EMAIL}" Name=email_verified,Value=true | jq .
```

### OpenSearch debugtask pool

```shell
OPENSEARCH_DEBUGTASK_POOL_ID=$(ave aws cognito-idp list-user-pools --max-results 60 \
  --query 'UserPools[?Name==`cdp-sirsi-opensearch-debugtask`].Id' --output text)
echo "OPENSEARCH_DEBUGTASK_POOL_ID=${OPENSEARCH_DEBUGTASK_POOL_ID}"

EMAIL="ali.bahman@goaco.com"

# OpenSearch debugtask pool
ave aws cognito-idp admin-create-user \
  --user-pool-id "${OPENSEARCH_DEBUGTASK_POOL_ID}" \
  --username "${EMAIL}" \
  --user-attributes Name=email,Value="${EMAIL}" Name=email_verified,Value=true | jq .
```

## Groups (recommended access control)

Groups are managed in Terraform. Use the CLI to add users to the appropriate group.

Add a user to a group:
```shell
EMAIL="ali.bahman@goaco.com"
GROUP_NAME="tools-s3-uploader"

ave aws cognito-idp admin-add-user-to-group \
  --user-pool-id "${POOL_ID}" \
  --username "${EMAIL}" \
  --group-name "${GROUP_NAME}" | jq .
```

List users in a group:

```shell
GROUP_NAME="tools-s3-uploader"

ave aws cognito-idp list-users-in-group \
  --user-pool-id "${POOL_ID}" \
  --group-name "${GROUP_NAME}" \
  --query 'Users[].{Username:Username,Status:UserStatus,Enabled:Enabled}' \
  --output table | jq .
```

## Reset a user password

```shell
EMAIL="ali.bahman@goaco.com"

ave aws cognito-idp admin-reset-user-password \
  --user-pool-id "${POOL_ID}" \
  --username "${EMAIL}" | jq .
```

For OpenSearch admin:

```shell
EMAIL="ali.bahman@goaco.com"

ave aws cognito-idp admin-reset-user-password \
  --user-pool-id "${OPENSEARCH_ADMIN_POOL_ID}" \
  --username "${EMAIL}" | jq .
```

For OpenSearch gateway:

```shell
EMAIL="ali.bahman@goaco.com"

ave aws cognito-idp admin-reset-user-password \
  --user-pool-id "${OPENSEARCH_GATEWAY_POOL_ID}" \
  --username "${EMAIL}" | jq .
```

For OpenSearch debugtask:

```shell
EMAIL="ali.bahman@goaco.com"

ave aws cognito-idp admin-reset-user-password \
  --user-pool-id "${OPENSEARCH_DEBUGTASK_POOL_ID}" \
  --username "${EMAIL}" | jq .
```

## Disable / enable a user

```shell
EMAIL="ali.bahman@goaco.com"

ave aws cognito-idp admin-disable-user \
  --user-pool-id "${POOL_ID}" \
  --username "${EMAIL}"

ave aws cognito-idp admin-enable-user \
  --user-pool-id "${POOL_ID}" \
  --username "${EMAIL}" | jq .
```

For OpenSearch admin:

```shell
EMAIL="ali.bahman@goaco.com"

ave aws cognito-idp admin-disable-user \
  --user-pool-id "${OPENSEARCH_ADMIN_POOL_ID}" \
  --username "${EMAIL}" | jq .

ave aws cognito-idp admin-enable-user \
  --user-pool-id "${OPENSEARCH_ADMIN_POOL_ID}" \
  --username "${EMAIL}" | jq .
```

For OpenSearch gateway:

```shell
EMAIL="ali.bahman@goaco.com"

ave aws cognito-idp admin-disable-user \
  --user-pool-id "${OPENSEARCH_GATEWAY_POOL_ID}" \
  --username "${EMAIL}" | jq .

ave aws cognito-idp admin-enable-user \
  --user-pool-id "${OPENSEARCH_GATEWAY_POOL_ID}" \
  --username "${EMAIL}" | jq .
```

For OpenSearch debugtask:

```shell
EMAIL="ali.bahman@goaco.com"

ave aws cognito-idp admin-disable-user \
  --user-pool-id "${OPENSEARCH_DEBUGTASK_POOL_ID}" \
  --username "${EMAIL}" | jq .

ave aws cognito-idp admin-enable-user \
  --user-pool-id "${OPENSEARCH_DEBUGTASK_POOL_ID}" \
  --username "${EMAIL}" | jq .
```

## List users

```shell
ave aws cognito-idp list-users \
  --user-pool-id "${POOL_ID}" \
  --query 'Users[].{Username:Username,Status:UserStatus,Enabled:Enabled}' \
  --output table | cat
```

For OpenSearch admin:

```shell
ave aws cognito-idp list-users \
  --user-pool-id "${OPENSEARCH_ADMIN_POOL_ID}" \
  --query 'Users[].{Username:Username,Status:UserStatus,Enabled:Enabled}' \
  --output table  | cat
```

For OpenSearch gateway:

```shell
ave aws cognito-idp list-users \
  --user-pool-id "${OPENSEARCH_GATEWAY_POOL_ID}" \
  --query 'Users[].{Username:Username,Status:UserStatus,Enabled:Enabled}' \
  --output table | cat
```

For OpenSearch debugtask:

```shell
ave aws cognito-idp list-users \
  --user-pool-id "${OPENSEARCH_DEBUGTASK_POOL_ID}" \
  --query 'Users[].{Username:Username,Status:UserStatus,Enabled:Enabled}' \
  --output table | cat
```

## Logout (hosted UI)

The `/logout` path on the tool domains is handled by OpenSearch and will **not** log out of Cognito.
Use the Cognito hosted UI logout endpoint instead:

```
https://<cognito-domain>/logout?client_id=<CLIENT_ID>&logout_uri=<ENCODED_RETURN_URL>
```

Examples:

- OpenSearch admin:
```
https://cdp-sirsi-<env>-opensearch-admin.auth.eu-west-2.amazoncognito.com/logout?client_id=<ADMIN_CLIENT_ID>&logout_uri=https%3A%2F%2Fopensearch-admin.<env>.supplier-information.find-tender.service.gov.uk%2F
```

- OpenSearch gateway:
```
https://cdp-sirsi-<env>-opensearch-gateway.auth.eu-west-2.amazoncognito.com/logout?client_id=<GATEWAY_CLIENT_ID>&logout_uri=https%3A%2F%2Fopensearch-gateway.<env>.supplier-information.find-tender.service.gov.uk%2F
```

- OpenSearch debugtask:
```
https://cdp-sirsi-<env>-opensearch-debugtask.auth.eu-west-2.amazoncognito.com/logout?client_id=<DEBUGTASK_CLIENT_ID>&logout_uri=https%3A%2F%2Fopensearch-debugtask.<env>.supplier-information.find-tender.service.gov.uk%2F
```

## Self-signup and approvals

The current pool configuration is **admin-create-only** (no public self‑signup). This means:
- Users do **not** choose their own password up front.
- They receive a temporary password via email and set a new password on first login.

Users can use the **Forgot password** flow after their first login, via the hosted UI for each tool.

If you want **self-signup with admin approval**, Cognito does **not** support that directly; it requires a custom workflow (Lambda triggers or a separate approval system).
