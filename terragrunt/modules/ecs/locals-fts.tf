locals {

  orchestrator_fts_service_version = data.aws_ssm_parameter.orchestrator_fts_service_version.value

  service_version_fts = var.pinned_service_version_fts == null ? local.orchestrator_fts_service_version : var.pinned_service_version_fts

  fts_secrets_arn = data.aws_secretsmanager_secret.fts_secrets.arn

  fts_one_login_logout_redirect_uris = {
    development = "https://fts.${var.public_domain}/auth/logout"
    staging     = "https://www-staging.find-tender.service.gov.uk/auth/logout"
    integration = "https://www-tpp.find-tender.service.gov.uk/auth/logout"
    production  = "https://www.find-tender.service.gov.uk/auth/logout"
  }

  fts_one_login_redirect_uris = {
    development = "https://fts.${var.public_domain}/auth/callback"
    staging     = "https://www-staging.find-tender.service.gov.uk/auth/callback"
    integration = "https://www-tpp.find-tender.service.gov.uk/auth/callback"
    production  = "https://www.find-tender.service.gov.uk/auth/callback"
  }

  fts_site_domains = {
    development = "fts.${var.public_domain}"
    staging     = "www-staging.find-tender.service.gov.uk"
    integration = "www-tpp.find-tender.service.gov.uk"
    production  = "www.find-tender.service.gov.uk"
  }

  fts_secrets = {
    email_contactus                           = "${local.fts_secrets_arn}:CONTACTUS_EMAIL::"
    email_e_enablement                        = "${local.fts_secrets_arn}:EENABLEMENT_EMAIL::"
    email_subscription_authentication_key     = "${local.fts_secrets_arn}:EMAIL_SUBSCRIPTION_AUTHENTICATION_KEY::"
    email_subscription_cipher                 = "${local.fts_secrets_arn}:EMAIL_SUBSCRIPTION_CIPHER::"
    email_tech_support                        = "${local.fts_secrets_arn}:TECHSUPPORT_EMAIL::"
    email_user_research                       = "${local.fts_secrets_arn}:USER_RESEARCH_EMAIL::"
    fts_one_login_client_id                   = local.one_login.credential_locations.client_id
    fts_srsi_api_key                          = "${local.fts_secrets_arn}:FTS_SRSI_API_KEY::"
    google_analytics_key                      = "${local.fts_secrets_arn}:GOOGLE_ANALYTICS_KEY::"
    google_tag_manager_key                    = "${local.fts_secrets_arn}:GOOGLE_TAG_MANAGER_KEY::"
    http_basic_auth_enabled                   = "${local.fts_secrets_arn}:HTTP_BASIC_AUTH_ENABLED::"
    http_basic_auth_pass                      = "${local.fts_secrets_arn}:HTTP_BASIC_AUTH_PASS::"
    http_basic_auth_user                      = "${local.fts_secrets_arn}:HTTP_BASIC_AUTH_USER::"
    letsencrypt_key_authorization             = "${local.fts_secrets_arn}:LETSENCRYPT_KEY_AUTHORIZATION::"
    letsencrypt_token                         = "${local.fts_secrets_arn}:LETSENCRYPT_TOKEN::"
    notify_api_key                            = "${local.fts_secrets_arn}:NOTIFY_API_KEY::"
    notify_template_id_general                = "${local.fts_secrets_arn}:NOTIFY_TEMPLATE_ID_GENERAL::"
    notify_template_id_saved_searches         = "${local.fts_secrets_arn}:NOTIFY_TEMPLATE_ID_SAVED_SEARCHES::"
    notify_template_id_watched_notice_closing = "${local.fts_secrets_arn}:NOTIFY_TEMPLATE_ID_WATCHED_NOTICE_CLOSING::"
    notify_template_id_watched_notice_update  = "${local.fts_secrets_arn}:NOTIFY_TEMPLATE_ID_WATCHED_NOTICE_UPDATE::"
    one_login_base_url                        = local.one_login.credential_locations.authority
    one_login_fln_api_key_arn                 = data.aws_secretsmanager_secret.one_login_forward_logout_notification_api_key.arn
    one_login_private_key                     = local.one_login.credential_locations.private_key
    run_guest_token                           = "${local.fts_secrets_arn}:RUN_GUEST_TOKEN::"
    run_migrator_token                        = "${local.fts_secrets_arn}:RUN_MIGRATOR_TOKEN::"
    run_registrar_token                       = "${local.fts_secrets_arn}:RUN_REGISTRAR_TOKEN::"
    ses_configuration_set_name                = var.ses_configuration_set_name
    srsi_logout_api_key                       = "${local.fts_secrets_arn}:SRSI_LOGOUT_API_KEY::"
    sso_api_authentication_key                = "${local.fts_secrets_arn}:SSO_API_AUTHENTICATION_KEY::"
    sso_cipher                                = "${local.fts_secrets_arn}:SSO_CIPHER::"
    sso_password                              = "${local.fts_secrets_arn}:SSO_PASSWORD::"
    sso_token_session_authentication_key      = "${local.fts_secrets_arn}:SSO_TOKEN_SESSION_AUTHENTICATION_KEY::"
    sso_token_session_cipher                  = "${local.fts_secrets_arn}:SSO_TOKEN_SESSION_CIPHER::"
  }

  fts_parameters = {
    email_support                       = var.is_production ? "noreply@find-tender.service.gov.uk" : "noreply@${var.public_domain}"
    dev_email                           = "${local.fts_secrets_arn}:DEV_EMAIL::"
    app_host_address                    = "%"
    buyer_corporate_identifier_prefixes = "sid4gov.cabinetoffice.gov.uk|supplierregistration.service.xgov.uk|test-idp-intra.nqc.com"
    cookie_domain                       = local.fts_site_domains[var.environment]
    database_schema                     = "cdp_sirsi_fts_cluster"
    db_host                             = var.db_fts_cluster_address
    db_name                             = var.db_fts_cluster_name
    database_server_address             = var.db_fts_cluster_address
    database_server_port                = 3306
    database_ssl                        = false
    environment                         = upper(var.environment)
    fts_allowed_target_email_domains    = join(",", var.fts_allowed_target_email_domains)
    fts_client_assertion_type           = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"
    fts_one_login_logout_redirect_uri   = local.fts_one_login_logout_redirect_uris[var.environment]
    fts_one_login_redirect_uri          = local.fts_one_login_redirect_uris[var.environment]
    licenced_to                         = "No-one"
    local_version                       = 1100
    session_name_default                = "SRSI_FT_AUTH"
    site_domain                         = local.fts_site_domains[var.environment]
    site_tag                            = "TEST"
    srsi_authority_token_endpoint       = "https://authority.${var.public_domain}/token"
    srsi_dashboard_endpoint             = "https://${var.public_domain}"
    srsi_organisation_lookup_endpoint   = "https://organisation.${var.public_domain}"
    srsi_tenant_lookup_endpoint         = "https://tenant.${var.public_domain}/tenant/lookup"
    ssl_service                         = true
    uk9_enabled                         = contains(["development"], var.environment) ? true : false
    use_srsi                            = true
    use_srsi_for_api                    = true
    valid_until                         = 1924990799
  }

  fts_service_paremeters = {
    container_port  = var.service_configs.fts.port
    cpu             = var.service_configs.fts.cpu
    host_port       = var.service_configs.fts.port
    image           = local.ecr_urls[var.service_configs.fts.name]
    lg_name         = aws_cloudwatch_log_group.tasks[var.service_configs.fts.name].name
    lg_prefix       = "app"
    lg_region       = data.aws_region.current.name
    memory          = var.is_production ? var.service_configs.fts.memory * 2 : var.service_configs.fts.memory // @TODO (ABN) Burn me
    name            = var.service_configs.fts.name
    public_domain   = var.public_domain
    service_version = local.service_version_fts
    vpc_cidr        = var.vpc_cider
  }

  fts_scheduler_service_paremeters = {
    container_port  = var.service_configs.fts_scheduler.port
    cpu             = var.service_configs.fts_scheduler.cpu
    host_port       = var.service_configs.fts_scheduler.port
    image           = local.ecr_urls[var.service_configs.fts_scheduler.name]
    lg_name         = aws_cloudwatch_log_group.tasks[var.service_configs.fts_scheduler.name].name
    lg_prefix       = "app"
    lg_region       = data.aws_region.current.name
    memory          = var.is_production ? var.service_configs.fts_scheduler.memory * 2 : var.service_configs.fts_scheduler.memory // @TODO (ABN) Burn me
    name            = var.service_configs.fts_scheduler.name
    public_domain   = var.public_domain
    service_version = local.service_version_fts
    vpc_cidr        = var.vpc_cider
  }

  fts_migrations_service_paremeters = {
    container_port  = var.service_configs.fts_migrations.port
    cpu             = var.service_configs.fts_migrations.cpu
    host_port       = var.service_configs.fts_migrations.port
    image           = local.ecr_urls[var.service_configs.fts_migrations.name]
    lg_name         = aws_cloudwatch_log_group.tasks[var.service_configs.fts_migrations.name].name
    lg_prefix       = "app"
    lg_region       = data.aws_region.current.name
    memory          = var.service_configs.fts_migrations.memory
    name            = var.service_configs.fts_migrations.name
    public_domain   = var.public_domain
    service_version = local.service_version_fts
    vpc_cidr        = var.vpc_cider
  }

  fts_container_parameters = merge(
    local.fts_parameters,
    local.fts_service_paremeters,
    local.fts_secrets
  )

  fts_scheduler_container_parameters = merge(
    local.fts_parameters,
    local.fts_scheduler_service_paremeters,
    local.fts_secrets
  )

  fts_migrations_container_parameters = merge(
    local.fts_parameters,
    local.fts_migrations_service_paremeters,
    local.fts_secrets
  )

  migrations_fts = ["fts-migrations"]

  migration_configs_fts = {
    for name, config in var.service_configs :
    config.name => config if contains(local.migrations_fts, config.name)
  }

}
