locals {

  aspcore_environment = "Aws${title(var.environment)}"

  cognito_enabled = contains(["development", "staging"], var.environment)

  db_sirsi_secret_arn = var.db_sirsi_cluster_credentials_arn
  db_ev_secret_arn    = var.db_ev_cluster_credentials_arn

  db_sirsi_password = "${local.db_sirsi_secret_arn}:password::"
  db_sirsi_username = "${local.db_sirsi_secret_arn}:username::"
  db_ev_password    = "${local.db_ev_secret_arn}:password::"
  db_ev_username    = "${local.db_ev_secret_arn}:username::"

  ecr_urls = {
    for task in local.tasks : task => "${local.orchestrator_account_id}.dkr.ecr.eu-west-2.amazonaws.com/cdp-${task}"
  }

  name_prefix = var.product.resource_name

  one_loging = {
    credential_locations = {
      account_url = "${data.aws_secretsmanager_secret.one_login_credentials.arn}:AccountUrl::"
      authority   = "${data.aws_secretsmanager_secret.one_login_credentials.arn}:Authority::"
      client_id   = "${data.aws_secretsmanager_secret.one_login_credentials.arn}:ClientId::"
      private_key = "${data.aws_secretsmanager_secret.one_login_credentials.arn}:PrivateKey::"
    }
  }

  orchestrator_account_id = var.account_ids["orchestrator"]

  orchestrator_service_version = data.aws_ssm_parameter.orchestrator_service_version.value

  service_version = var.pinned_service_version == null ? data.aws_ssm_parameter.orchestrator_service_version.value : var.pinned_service_version

  shared_sessions_enabled    = true
  ssm_data_protection_prefix = "${local.name_prefix}-ec-sessions"

  migrations = ["organisation-information-migrations", "entity-verification-migrations"]

  migration_configs = {
    for name, config in var.service_configs :
    config.name => config if contains(local.migrations, config.name)
  }

  outbox_processors_desire_count = 1 # won't be scalable.

  send_notify_emails_enabled_accounts = ["development", "staging", "integration", "production"]
  send_notify_emails                  = contains(local.send_notify_emails_enabled_accounts, var.environment)

  service_configs = {
    for name, config in var.service_configs :
    config.name => config if !contains(local.migrations, config.name)
  }

  tasks = [
    for name, config in var.service_configs :
    config.name
  ]

  waf_enabled = true

}
