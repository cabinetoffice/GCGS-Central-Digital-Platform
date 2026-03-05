locals {

  aspcore_environment          = "Aws${title(var.environment)}"
  cognito_enabled              = contains(["development", "staging"], var.environment)
  fts_cluster_id               = aws_ecs_cluster.fts.id
  fts_cluster_name             = aws_ecs_cluster.fts.name
  fts_ecs_listener_arn         = aws_lb_listener.ecs_fts.arn
  internal_prefix              = length("internal.${var.public_domain}") > 64 ? "in" : "internal"
  internal_domain              = "${local.internal_prefix}.${var.public_domain}"
  internal_ecs_listener_arn    = aws_lb_listener.ecs_internal.arn
  use_internal_service_urls    = var.use_internal_service_urls != null ? var.use_internal_service_urls : contains(["development"], var.environment)
  use_internal_issuer          = var.use_internal_issuer != null ? var.use_internal_issuer : contains(["development"], var.environment)
  main_cluster_id              = aws_ecs_cluster.this.id
  main_cluster_name            = aws_ecs_cluster.this.name
  main_ecs_listener_arn        = aws_lb_listener.ecs.arn
  name_prefix                  = var.product.resource_name
  name_prefix_fts              = "${local.name_prefix}-fts"
  name_prefix_php              = "${local.name_prefix}-php"
  php_cluster_id               = aws_ecs_cluster.that.id
  php_cluster_name             = aws_ecs_cluster.that.name
  php_ecs_listener_arn         = aws_lb_listener.ecs_php.arn
  unauthenticated_assets_paths = ["/one-login/back-channel-sign-out", "/assets/*", "/css/*", "/manifest.json"]

  db_ev_password      = "${local.db_ev_secret_arn}:password::"
  db_ev_secret_arn    = var.db_ev_cluster_credentials_arn
  db_ev_username      = "${local.db_ev_secret_arn}:username::"
  db_fts_password     = "${local.db_fts_secret_arn}:password::"
  db_fts_secret_arn   = var.db_fts_cluster_credentials_arn
  db_fts_username     = "${local.db_fts_secret_arn}:username::"
  db_sirsi_password   = "${local.db_sirsi_secret_arn}:password::"
  db_sirsi_secret_arn = var.db_sirsi_cluster_credentials_arn
  db_sirsi_username   = "${local.db_sirsi_secret_arn}:username::"

  ecr_urls = {
    for task in local.tasks : task => "${local.orchestrator_account_id}.dkr.ecr.eu-west-2.amazonaws.com/cdp-${task}"
  }

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

  migration_configs_sirsi = {
    for name, config in var.service_configs :
    config.name => config if config.type == "db-migration" && config.cluster == "sirsi"
  }

  send_notify_emails_enabled_accounts = ["development", "staging", "integration", "production"]
  send_notify_emails                  = contains(local.send_notify_emails_enabled_accounts, var.environment)

  service_configs = {
    for name, config in var.service_configs :
    config.name => {
      for key, value in config :
      key => value if value != null
    } if config.type != "db-migration"
  }

  internal_service_urls = {
    for name, config in local.service_configs :
    config.name => "https://${config.name}.${local.internal_domain}"
  }

  public_service_urls = {
    for name, config in local.service_configs :
    config.name => "https://${config.name}.${var.public_domain}"
  }

  service_configs_php = {
    for name, config in var.service_configs :
    config.name => {
      for key, value in config :
      key => value if value != null
    } if config.type != "db-migration" && config.cluster == "sirsi-php"
  }

  service_configs_fts = {
    for name, config in var.service_configs :
    config.name => {
      for key, value in config :
      key => value if value != null
    } if config.type != "db-migration" && config.cluster == "fts"
  }

  service_ports = {
    "cluster:sirsi"     = 8080
    "cluster:fts"       = 8080
    "cluster:sirsi-php" = 8070
    "service:cfs"       = 8060
  }

  service_ports_by_service = {
    for name, config in var.service_configs :
    config.name => lookup(
      local.service_ports,
      "service:${config.name}",
      local.service_ports["cluster:${config.cluster}"]
    )
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
