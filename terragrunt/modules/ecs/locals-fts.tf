locals {

  orchestrator_fts_service_version = data.aws_ssm_parameter.orchestrator_fts_service_version.value

  service_version_fts = var.pinned_service_version_fts == null ? local.orchestrator_fts_service_version : var.pinned_service_version_fts

  fts_secrets_arn = data.aws_secretsmanager_secret.fts_secrets.arn

  fts_notice_publish_internal_key_arn = aws_secretsmanager_secret.fts_notice_publish_internal_key.arn

  fts_site_domains = {
    development = "fts.${var.public_domain}"
    staging     = "www-staging.find-tender.service.gov.uk"
    integration = "www-tpp.find-tender.service.gov.uk"
    production  = "www.find-tender.service.gov.uk"
  }

  fts_redirect_domains = {
    development = "fts-app.${var.public_domain}"
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
    fts_sirsi_api_key                         = "${local.fts_secrets_arn}:FTS_SIRSI_API_KEY::"
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
    app_host_address                      = "%"
    async_enabled                         = var.is_production ? false : true
    buyer_corporate_identifier_prefixes   = "sid4gov.cabinetoffice.gov.uk|supplierregistration.service.xgov.uk|test-idp-intra.nqc.com"
    cookie_domain                         = local.fts_site_domains[var.environment]
    database_schema                       = "cdp_sirsi_fts_cluster"
    database_server_address               = var.db_fts_cluster_address
    database_server_port                  = 3306
    database_ssl                          = false
    db_host                               = var.db_fts_cluster_address
    db_name                               = var.db_fts_cluster_name
    dev_email                             = "${local.fts_secrets_arn}:DEV_EMAIL::"
    dotnet_ui_app_url                     = "https://${local.fts_redirect_domains[var.environment]}"
    email_support                         = var.is_production ? "noreply@find-tender.service.gov.uk" : "noreply@${var.public_domain}"
    environment                           = upper(var.environment)
    fts_allowed_target_email_domains      = join(",", local.fts_allowed_target_email_domains[var.environment])
    fts_client_assertion_type             = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"
    fts_one_login_logout_redirect_uri     = "https://${local.fts_site_domains[var.environment]}/auth/logout"
    fts_one_login_redirect_uri            = "https://${local.fts_site_domains[var.environment]}/auth/callback"
    licenced_to                           = "No-one"
    local_version                         = 1100
    modernised_landing_page               = true
    summarised_search_enabled             = contains(["development", "staging"], var.environment)
    pa23_enabled                          = true
    notice_publish_internal_key           = local.fts_notice_publish_internal_key_arn
    notice_publish_queue_url              = var.queue_fts_notice_publish_url
    notice_render_queue_url               = var.queue_fts_notice_render_url
    notice_render_cache_bucket            = module.s3_bucket_fts_notice_render_cache.bucket
    notice_render_cache_cdn_url           = var.cloudfront_downloads_enabled ? "https://${module.cloudfront_fts_notice_render_cache.cloudfront_domain_name}" : ""
    notice_render_cache_debug_marker      = var.is_production ? false : true
    notice_render_cache_enabled           = var.is_production ? false : true
    notice_render_worker_enabled          = var.environment == "development"
    session_name_default                  = "SRSI_FT_AUTH"
    site_domain                           = local.fts_site_domains[var.environment]
    site_tag                              = "TEST"
    srsi_authority_token_endpoint         = "https://authority.${var.public_domain}/token"
    srsi_dashboard_endpoint               = "https://${var.public_domain}"
    srsi_organisation_lookup_endpoint     = "https://organisation.${var.public_domain}"
    srsi_tenant_lookup_endpoint           = "https://tenant.${var.public_domain}/tenant/lookup"
    ssl_service                           = true
    submission_log_globally_enabled       = contains(["integration"], var.environment)
    uk11_240_enabled                      = true
    uk17_enable_current_reporting_periods = !contains(["development", "production"], var.environment)
    uk17_enabled                          = true
    uk1_notices_rebuild_enabled           = contains(["development", "staging"], var.environment)
    uk2_notices_rebuild_enabled           = contains(["development", "staging"], var.environment)
    uk3_notices_rebuild_enabled           = contains(["development", "staging"], var.environment)
    uk6_notices_rebuild_enabled           = contains(["development", "staging"], var.environment)
    uk9_enabled                           = true
    use_srsi                              = true
    use_srsi_for_api                      = true
    valid_until                           = 1924990799
    validation_weight                     = contains(["integration"], var.environment) ? "QUAL_WEIGHT" : "PROD_WEIGHT"
  }


  fts_dotnet_common = {
    aspcore_environment = local.aspcore_environment
    lg_prefix           = "app"
    lg_region           = data.aws_region.current.region
    service_version     = local.service_version_fts
  }

  fts_dotnet_fts_app = merge(
    local.fts_dotnet_common,
    {
      fts_service_url             = local.fts_service_url
      onelogin_authority          = local.one_login.credential_locations.authority
      onelogin_client_id          = local.one_login.credential_locations.client_id
      onelogin_private_key        = local.one_login.credential_locations.private_key
      db_pg_address               = var.db_find_a_tender_cluster_address
      db_pg_name                  = var.db_find_a_tender_cluster_name
      db_pg_password              = local.db_find_a_tender_password
      db_pg_port                  = 5432
      db_pg_username              = local.db_find_a_tender_username
      db_mysql_address            = var.db_fts_cluster_address
      db_mysql_name               = var.db_fts_cluster_name
      db_mysql_password           = local.db_fts_password
      db_mysql_port               = 3306
      db_mysql_username           = local.db_fts_username
      notice_publish_internal_key = local.fts_notice_publish_internal_key_arn
      public_domain               = var.public_domain
      fts_sirsi_api_key           = "${local.fts_secrets_arn}:FTS_SIRSI_API_KEY::"
      vpc_cidr                    = var.vpc_cider
    }
  )

  fts_dotnet_search_api = merge(
    local.fts_dotnet_common,
    {
      opensearch_endpoint = "https://${var.opensearch_endpoint}"
    }
  )

  fts_dotnet_search_indexer = merge(
    local.fts_dotnet_common,
    {
      db_address          = var.db_fts_cluster_address
      db_name             = var.db_fts_cluster_name
      db_password         = local.db_fts_password
      db_port             = 3306
      db_username         = local.db_fts_username
      db_pg_address       = var.db_find_a_tender_cluster_address
      db_pg_name          = var.db_find_a_tender_cluster_name
      db_pg_password      = local.db_find_a_tender_password
      db_pg_port          = 5432
      db_pg_username      = local.db_find_a_tender_username
      opensearch_endpoint = "https://${var.opensearch_endpoint}"
    }
  )

  fts_dotnet_user_api = merge(
    local.fts_dotnet_common,
    {
      db_address                  = var.db_find_a_tender_cluster_address
      db_name                     = var.db_find_a_tender_cluster_name
      db_password                 = local.db_find_a_tender_password
      db_port                     = 5432
      db_username                 = local.db_find_a_tender_username
      sirsi_authority_api_baseurl = local.use_internal_service_urls ? local.internal_service_urls["authority"] : local.public_service_urls["authority"]
      sirsi_person_api_baseurl    = local.use_internal_service_urls ? local.internal_service_urls["person"] : local.public_service_urls["person"]
    }
  )

  fts_dotnet_job_scheduler = merge(
    local.fts_dotnet_common,
    {
      db_address  = var.db_find_a_tender_cluster_address
      db_name     = var.db_find_a_tender_cluster_name
      db_password = local.db_find_a_tender_password
      db_port     = 5432
      db_username = local.db_find_a_tender_username
    }
  )

  fts_service_parameters = {
    service_port    = local.service_ports_by_service[var.service_configs.fts.name]
    cpu             = var.service_configs.fts.cpu
    image           = local.ecr_urls[var.service_configs.fts.name]
    lg_name         = aws_cloudwatch_log_group.tasks[var.service_configs.fts.name].name
    lg_prefix       = "app"
    lg_region       = data.aws_region.current.region
    memory          = var.is_production ? var.service_configs.fts.memory * 2 : var.service_configs.fts.memory // @TODO (ABN) Burn me
    name            = var.service_configs.fts.name
    public_domain   = var.public_domain
    service_version = local.service_version_fts
    vpc_cidr        = var.vpc_cider
  }

  fts_scheduler_service_parameters = {
    service_port    = local.service_ports_by_service[var.service_configs.fts_scheduler.name]
    cpu             = var.service_configs.fts_scheduler.cpu
    image           = local.ecr_urls[var.service_configs.fts_scheduler.name]
    lg_name         = aws_cloudwatch_log_group.tasks[var.service_configs.fts_scheduler.name].name
    lg_prefix       = "app"
    lg_region       = data.aws_region.current.region
    memory          = var.is_production ? var.service_configs.fts_scheduler.memory * 2 : var.service_configs.fts_scheduler.memory // @TODO (ABN) Burn me
    name            = var.service_configs.fts_scheduler.name
    public_domain   = var.public_domain
    service_version = local.service_version_fts
    vpc_cidr        = var.vpc_cider
  }

  fts_migrations_service_parameters = {
    service_port    = local.service_ports_by_service[var.service_configs.fts_migrations.name]
    cpu             = var.service_configs.fts_migrations.cpu
    image           = local.ecr_urls[var.service_configs.fts_migrations.name]
    lg_name         = aws_cloudwatch_log_group.tasks[var.service_configs.fts_migrations.name].name
    lg_prefix       = "app"
    lg_region       = data.aws_region.current.region
    memory          = var.service_configs.fts_migrations.memory
    name            = var.service_configs.fts_migrations.name
    public_domain   = var.public_domain
    service_version = local.service_version_fts
    vpc_cidr        = var.vpc_cider
  }

  fts_container_parameters = merge(
    local.fts_parameters,
    local.fts_service_parameters,
    local.fts_secrets
  )

  fts_scheduler_container_parameters = merge(
    local.fts_parameters,
    local.fts_scheduler_service_parameters,
    local.fts_secrets
  )

  fts_migrations_container_parameters = merge(
    local.fts_parameters,
    local.fts_migrations_service_parameters,
    local.fts_secrets
  )

  fts_findtender_migrations_container_parameters = merge(
    local.fts_dotnet_common,
    {
      aspcore_environment = local.aspcore_environment
      db_pg_address       = var.db_find_a_tender_cluster_address
      db_pg_name          = var.db_find_a_tender_cluster_name
      db_pg_password      = local.db_find_a_tender_password
      db_pg_port          = 5432
      db_pg_username      = local.db_find_a_tender_username
      cpu                 = var.service_configs.fts_findtender_migrations.cpu
      image               = local.ecr_urls[var.service_configs.fts_findtender_migrations.name]
      lg_name             = aws_cloudwatch_log_group.tasks[var.service_configs.fts_findtender_migrations.name].name
      lg_prefix           = "db"
      lg_region           = data.aws_region.current.region
      memory              = var.service_configs.fts_findtender_migrations.memory
      name                = var.service_configs.fts_findtender_migrations.name
      service_version     = local.service_version_fts
    }
  )

  fts_notice_publish_worker_service_parameters = {
    service_port    = local.service_ports_by_service[var.service_configs.fts_notice_publish_worker.name]
    cpu             = var.service_configs.fts_notice_publish_worker.cpu
    image           = local.ecr_urls[var.service_configs.fts_notice_publish_worker.name]
    lg_name         = aws_cloudwatch_log_group.tasks[var.service_configs.fts_notice_publish_worker.name].name
    lg_prefix       = "app"
    lg_region       = data.aws_region.current.region
    memory          = var.is_production ? var.service_configs.fts_notice_publish_worker.memory * 2 : var.service_configs.fts_notice_publish_worker.memory // @TODO (ABN) Burn me
    name            = var.service_configs.fts_notice_publish_worker.name
    public_domain   = var.public_domain
    service_version = local.service_version_fts
    vpc_cidr        = var.vpc_cider
  }

  fts_notice_publish_worker_container_parameters = merge(
    local.fts_parameters,
    local.fts_notice_publish_worker_service_parameters,
    local.fts_secrets
  )

  fts_notice_render_worker_service_parameters = {
    service_port    = local.service_ports_by_service[var.service_configs.fts_notice_render_worker.name]
    cpu             = var.service_configs.fts_notice_render_worker.cpu
    image           = local.ecr_urls[var.service_configs.fts_notice_render_worker.name]
    lg_name         = aws_cloudwatch_log_group.tasks[var.service_configs.fts_notice_render_worker.name].name
    lg_prefix       = "app"
    lg_region       = data.aws_region.current.region
    memory          = var.service_configs.fts_notice_render_worker.memory
    name            = var.service_configs.fts_notice_render_worker.name
    public_domain   = var.public_domain
    service_version = local.service_version_fts
    vpc_cidr        = var.vpc_cider
  }

  fts_notice_render_worker_container_parameters = merge(
    local.fts_parameters,
    local.fts_notice_render_worker_service_parameters,
    local.fts_secrets,
    {
      notice_render_dlq_url        = var.queue_fts_notice_render_dlq_url
      notice_render_queue_url      = var.queue_fts_notice_render_url
    }
  )

  fts_allowed_target_email_domains = {
    development = [
      "goaco.com"
    ]
    staging = [
      "cabinetoffice.gov.uk",
      "goaco.com"
    ]
    integration = [
      "ace-advice.co.uk",
      "adamprocure.co.uk",
      "adb.co.uk",
      "ansarada.com",
      "atamis.co.uk",
      "axians.com",
      "bidful.com",
      "bipsolutions.com",
      "cabinetoffice.gov.uk",
      "cloudsmiths.co.za",
      "crowncommercial.gov.uk",
      "dft.gov.uk",
      "dotmodus.com",
      "eu-supply.com",
      "eurodyn.com",
      "evosysglobal.com",
      "excelerateds2p.com",
      "fcdo.gov.uk",
      "fusionpractices.com",
      "gep.com",
      "goaco.com",
      "gov.wales",
      "homeoffice.gov.uk",
      "in-tend.co.uk",
      "ivalua.com",
      "jaggaer.com",
      "justice.gov.uk",
      "Kainos.com",
      "kcfmcglobal.com",
      "klickstream.co.uk",
      "maistro.com",
      "mastek.com",
      "mastersoftware.co.uk",
      "mercell.com",
      "mod.gov.uk",
      "mytenders.co.uk",
      "nda.gov.uk",
      "nepo.org",
      "nhs.net",
      "oneadvanced.com",
      "panacea-software.com",
      "proactis.com",
      "proactisinterfaces.com",
      "publiccontractsscotland.gov.uk",
      "sell2wales.gov.wales",
      "sourcedogg.com",
      "sproc.net",
      "supplychainpartner.com",
      "tcs.com",
      "tenderlake.com",
      "theupside.io",
      "trisaas.com",
      "useadam-tech.com",
      "useadam.co.uk",
      "waxdigital.com",
      "xansium.com"
    ]
    production = []
  }

  fts_service_allowed_origins = {
    development = [
      "http://localhost:3000"
    ]
    staging = [
      "https://fts.staging.supplier-information.find-tender.service.gov.uk",
    ]
    integration = [
      "https://fts.integration.supplier-information.find-tender.service.gov.uk",
      "https://test-findtender.nqc.com",
      "https://truk-alpha.nqc.com",
      "https://truk-performance.nqc.com",
      "https://truk-prod.nqc.com",
      "https://www-integration.find-tender.service.gov.uk",
      "https://www-preview.find-tender.service.gov.uk",
      "https://www-tpp-preview.find-tender.service.gov.uk",
      "https://www-tpp.find-tender.service.gov.uk",
    ]
    production = [
      "https://fts.supplier-information.find-tender.service.gov.uk",
      "https://www.find-tender.service.gov.uk"
    ]
  }
}
