locals {

  aspcore_environment = "Aws${title(var.environment)}"

  cognito_enabled = contains(["development", "staging"], var.environment)

  db_ev_secret_arn    = var.db_ev_cluster_credentials_arn
  db_fts_secret_arn   = var.db_fts_cluster_credentials_arn
  db_sirsi_secret_arn = var.db_sirsi_cluster_credentials_arn

  db_ev_password    = "${local.db_ev_secret_arn}:password::"
  db_ev_username    = "${local.db_ev_secret_arn}:username::"
  db_fts_password   = "${local.db_fts_secret_arn}:password::"
  db_fts_username   = "${local.db_fts_secret_arn}:username::"
  db_sirsi_password = "${local.db_sirsi_secret_arn}:password::"
  db_sirsi_username = "${local.db_sirsi_secret_arn}:username::"

  ecr_urls = {
    for task in local.tasks : task => "${local.orchestrator_account_id}.dkr.ecr.eu-west-2.amazonaws.com/cdp-${task}"
  }

  name_prefix = var.product.resource_name

  one_login = {
    credential_locations = {
      account_url = "${data.aws_secretsmanager_secret.one_login_credentials.arn}:AccountUrl::"
      authority   = "${data.aws_secretsmanager_secret.one_login_credentials.arn}:Authority::"
      client_id   = "${data.aws_secretsmanager_secret.one_login_credentials.arn}:ClientId::"
      private_key = "${data.aws_secretsmanager_secret.one_login_credentials.arn}:PrivateKey::"
    }
  }

  orchestrator_account_id = var.account_ids["orchestrator"]

  orchestrator_sirsi_service_version = data.aws_ssm_parameter.orchestrator_sirsi_service_version.value

  service_version_sirsi = var.pinned_service_version_sirsi == null ? local.orchestrator_sirsi_service_version : var.pinned_service_version_sirsi

  shared_sessions_enabled    = true
  ssm_data_protection_prefix = "${local.name_prefix}-ec-sessions"
  ssm_commercial_tools_data_protection_prefix = "${local.name_prefix}-ec-ct-sessions"

  migrations_sirsi = ["organisation-information-migrations", "entity-verification-migrations"]

  migration_configs_sirsi = {
    for name, config in var.service_configs :
    config.name => config if contains(local.migrations_sirsi, config.name)
  }

  send_notify_emails_enabled_accounts = ["development", "staging", "integration", "production"]
  send_notify_emails                  = contains(local.send_notify_emails_enabled_accounts, var.environment)

  migrations_all = concat(local.migrations_sirsi, local.migrations_fts, local.migrations_cfs)
  service_configs = {
    for name, config in var.service_configs :
    config.name => config if !contains(local.migrations_all, config.name)
  }

  tasks = [
    for name, config in var.service_configs :
    config.name
  ]

  waf_enabled = true

  # @TODO (ABN) DP-1747 Remove env-based logic and these locals as well as old source of FTS configs once migration is completed
  fts_service_url                   = var.environment == "development" ? "https://fts.${var.public_domain}" : data.aws_secretsmanager_secret_version.fts_service_url.secret_string
  onelogin_logout_notification_urls = var.environment == "development" ? "https://fts.${var.public_domain}" : join(",", var.onelogin_logout_notification_urls)

  ses_identity_domain = var.is_production ? replace(var.public_domain, "supplier-information.", "") : var.public_domain


}
