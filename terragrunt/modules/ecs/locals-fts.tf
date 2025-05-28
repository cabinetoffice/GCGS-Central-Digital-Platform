locals {

  fts_screts_arn = data.aws_secretsmanager_secret.fts_secrets.arn

  fts_secrets = {
    api_email                             = "${local.fts_screts_arn}:API_EMAIL::"
    api_password                          = "${local.fts_screts_arn}:API_PASSWORD::"
    api_username                          = "${local.fts_screts_arn}:API_USERNAME::"
    contactus_email                       = "${local.fts_screts_arn}:CONTACTUS_EMAIL::"
    dev_email                             = "${local.fts_screts_arn}:DEV_EMAIL::"
    dropbox_access_token                  = "${local.fts_screts_arn}:DROPBOX_ACCESS_TOKEN::"
    email_subscription_authentication_key = "${local.fts_screts_arn}:EMAIL_SUBSCRIPTION_AUTHENTICATION_KEY::"
    email_subscription_cipher             = "${local.fts_screts_arn}:EMAIL_SUBSCRIPTION_CIPHER::"
    fts_one_login_client_id               = "${local.fts_screts_arn}:FTS_ONE_LOGIN_CLIENT_ID::"
    fts_srsi_api_key                      = "${local.fts_screts_arn}:FTS_SRSI_API_KEY::"
    google_analytics_key                  = "${local.fts_screts_arn}:GOOGLE_ANALYTICS_KEY::"
    google_tag_manager_key                = "${local.fts_screts_arn}:GOOGLE_TAG_MANAGER_KEY::"
    one_login_base_url                    = "${local.fts_screts_arn}:ONE_LOGIN_BASE_URL::"
    one_login_private_key                 = "${local.fts_screts_arn}:ONE_LOGIN_PRIVATE_KEY::"
    run_guest_token                       = "${local.fts_screts_arn}:RUN_GUEST_TOKEN::"
    run_migrator_token                    = "${local.fts_screts_arn}:RUN_MIGRATOR_TOKEN::"
    run_registrar_token                   = "${local.fts_screts_arn}:RUN_REGISTRAR_TOKEN::"
    srsi_logout_api_key                   = "${local.fts_screts_arn}:SRSI_LOGOUT_API_KEY::"
    sso_api_authentication_key            = "${local.fts_screts_arn}:SSO_API_AUTHENTICATION_KEY::"
    sso_cipher                            = "${local.fts_screts_arn}:SSO_CIPHER::"
    sso_home_page_auto_login              = "${local.fts_screts_arn}:SSO_HOME_PAGE_AUTO_LOGIN::"
    sso_password                          = "${local.fts_screts_arn}:SSO_PASSWORD::"
    sso_token_session_authentication_key  = "${local.fts_screts_arn}:SSO_TOKEN_SESSION_AUTHENTICATION_KEY::"
    sso_token_session_cipher              = "${local.fts_screts_arn}:SSO_TOKEN_SESSION_CIPHER::"
  }

  fts_parameters = {
    aliasemail_service                  = true
    app_host_address                    = "%"
    buyer_corporate_identifier_prefixes = "sid4gov.cabinetoffice.gov.uk|supplierregistration.service.xgov.uk|test-idp-intra.nqc.com"
    cookie_domain                       = "localhost"
    database_encryption                 = true
    database_replication_service        = false
    database_schema                     = "fts"
    db_host                             = var.db_fts_cluster_address
    db_name                             = var.db_fts_cluster_name
    db_pass                             = local.db_fts_password
    db_user                             = local.db_fts_username
    database_server_address             = var.db_fts_cluster_address
    database_server_port                = 3306
    database_ssl                        = false
    demo                                = true
    environment                         = var.environment
    fts_client_assertion_type           = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"
    fts_one_login_logout_redirect_uri   = "https://authority.${var.public_domain}/auth/logout"
    fts_one_login_redirect_uri          = "https://authority.${var.public_domain}/auth/callback"
    http_proxy_host                     = "waf.nqc.com" # @TODO (ABN) why?
    http_proxy_port                     = 8080
    include_devel                       = false
    licence_filter                      = 9223372036854775807 # Was set to PHP_INT_MAX - but we need to find out what should be
    licenced_to                         = "No-one"
    local_version                       = 1100
    session_name_default                = "SRSI_FT_AUTH"
    site_domain                         = var.public_domain
    site_tag                            = "TEST"
    srsi_authority_token_endpoint       = "https://authority.${var.public_domain}/token"
    srsi_dashboard_endpoint             = "https://${var.public_domain}"
    srsi_organisation_lookup_endpoint   = "https://organisation.${var.public_domain}"
    srsi_tenant_lookup_endpoint         = "https://tenant.${var.public_domain}/tenant/lookup"
    ssl_service                         = false
    two_factor_auth_service             = false
    use_proxy                           = false
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
    memory          = var.service_configs.fts.memory
    name            = var.service_configs.fts.name
    public_domain   = var.public_domain
    service_version = "latest" //local.service_version
    vpc_cidr        = var.vpc_cider
  }

  fts_container_parameters = merge(
    local.fts_parameters,
    local.fts_service_paremeters,
    local.fts_secrets
  )
}