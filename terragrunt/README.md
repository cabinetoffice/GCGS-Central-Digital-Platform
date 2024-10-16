# Central Digital Platform Infrastructure

This code base is responsible for provisioning the AWS infrastructure needed to support the CDP SIRSI application.

> **Note:** In this documentation, `ave` is an alias for the `aws-vault exec` command, and `aws-switch-to-*` is an alias that configures the following:
> - `AWS_PROFILE`
> - `TG_ENVIRONMENT`
> - `AWS_ENV` (optional)
> - `MFA_TOKEN` handler (out of scope for this documentation)
>
> You are welcome to use any profile manager or tool you are more comfortable with.

## Table of Contents
- [Bootstrap New Account](./docs/bootstap-new-account.md)
- [Create New User](./docs/bootstap-new-account.md#create-new-users)
- [Manage Secrets](./docs/manage-secrets.md)
   - [Retrieve Diagnostic URI](./docs/manage-secrets.md#retrieve-diagnostic-uri)
   - [Update Companies House Secrets](./docs/manage-secrets.md#update-companies-house-secrets)
   - [Update FtsService URL](./docs/manage-secrets.md#update-ftsservice-url)
   - [Update GOVUKNotify ApiKey](./docs/manage-secrets.md#update-govuknotify-apikey)
   - [Update GOVUKNotify Support Admin Email](./docs/manage-secrets.md#update-govuknotify-support-admin-email)
   - [Update OneLogin Secrets](./docs/manage-secrets.md#update-onelogin-secrets)
   - [Update Slack Configuration](./docs/manage-secrets.md#update-slack-configuration)
- [Pin Application/Service Version](./docs/bootstap-new-account.md#pin-applicationservice-version)
- [Run Databases' Migrations](./docs/bootstap-new-account.md#run-databases-migrations)
