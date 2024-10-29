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


---

# PGAdmin Temporary User Management Handbook

> To be removed as soon as possible

This handbook provides guidance on managing users in the production database account during the private-beta phase. Since the application is currently unable to handle all data management tasks, manual interventions by a number of developers and support users are necessary. Use the following commands to review, update, and manage users securely and efficiently.

## Useful Queries

### 1. Review

#### List Users
Use the following query to list all users, along with key details such as user ID, database creation privileges, superuser status, and password expiration.

```sql
SELECT
    usename AS username,
    usesysid AS user_id,
    usecreatedb AS can_create_db,
    usesuper AS is_superuser,
    valuntil AS password_expiration
FROM
    pg_user;
```

### 2. Update

#### Reset User Password
To reset a user’s password, replace `username` with the actual username and `new_password` with the desired new password.

```sql
ALTER USER username WITH PASSWORD 'new_password';
```

### 3. Create

#### Add New User
To create a new user, replace `new_user` with the desired username and `your_password` with the user’s password.

```sql
CREATE USER new_user WITH PASSWORD 'your_password';
```

### 4. Remove

#### Delete User
To delete a user, use the following command, replacing `username` with the specific username. The `IF EXISTS` clause avoids errors if the user does not exist.

```sql
DROP USER IF EXISTS username;
```

### 5. Revoke Privileges

To revoke all privileges for a user on all tables within a schema, use the following command, replacing `username` with the actual username. This is useful when restricting access to certain data.

```sql
REVOKE ALL PRIVILEGES ON ALL TABLES IN SCHEMA public FROM username;
```

### 6. Force Disconnect

To disconnect a user actively connected to the database, use the following query, replacing `username` with the actual username. This is useful when needing to make immediate changes to user access.

```sql
SELECT pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE usename = 'username';
```

---
