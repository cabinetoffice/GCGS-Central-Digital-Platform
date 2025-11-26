# Central Digital Platform Infrastructure
Dummy terragrunt change
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
- [Diagrams](./docs/diagrams.md)
  - [CI/CD Build and Deployment Pipelines](./docs/diagrams.md#cicd-build-and-deployment-pipelines)
  - [ECS and RDS Database Integration](./docs/diagrams.md#cdp-sirsi-application-ecs-and-rds-database-integration)
  - [ECS and SQS Queue Integration](./docs/diagrams.md#cdp-sirsi-application-ecs-and-sqs-queue-integration)
  - [High-Level Overview (DNS, Networking, ECS, and Data Flow)](#cdp-sirsi-application-high-level-overview-of-dns-networking-ecs-and-data-flow)
- [Manage Secrets](./docs/manage-secrets.md)
   - [Retrieve Diagnostic URI](./docs/manage-secrets.md#retrieve-diagnostic-uri)
   - [Update Charity Commission Secrets](./docs/manage-secrets.md#update-charity-commission-secrets)
   - [Update Companies House Secrets](./docs/manage-secrets.md#update-companies-house-secrets)
   - [Update FtsService URL](./docs/manage-secrets.md#update-ftsservice-url)
   - [Update GOVUKNotify ApiKey](./docs/manage-secrets.md#update-govuknotify-apikey)
   - [Update GOVUKNotify Support Admin Email](./docs/manage-secrets.md#update-govuknotify-support-admin-email)
   - [Update ODI Data Platform Secrets](./docs/manage-secrets.md#update-odi-data-platform-secret)
   - [Update OneLogin Forward Logout Notification API Key](./docs/manage-secrets.md#update-onelogin-forward-logout-notification-api-key)
   - [Update OneLogin Secrets](./docs/manage-secrets.md#update-onelogin-secrets)
   - [Update Pen Testing Configuration](./docs/manage-secrets.md#update-pen-testing-configuration)
   - [Update Production Database Users](./docs/manage-secrets.md#update-production-database-users)
   - [Update Slack Configuration](./docs/manage-secrets.md#update-slack-configuration)
   - [Update Terraform Operators](./docs/manage-secrets.md#update-terraform-operators)
   - [Update WAF Allowed IP Set](./docs/manage-secrets.md#update-waf-allowed-ip-set)
- [Pin Application/Service Version](./docs/bootstap-new-account.md#pin-applicationservice-version)
- [Run Databases' Migrations](./docs/bootstap-new-account.md#run-databases-migrations)
