locals {

  orchestrator_cfs_service_version = data.aws_ssm_parameter.orchestrator_cfs_service_version.value

  service_version_cfs = var.pinned_service_version_cfs == null ? local.orchestrator_cfs_service_version : var.pinned_service_version_cfs

  cfs_secrets_arn = data.aws_secretsmanager_secret.cfs_secrets.arn

  cfs_site_domains = {
    development = "cfs.${var.public_domain}"
    staging     = "www-preview.contractsfinder.service.gov.uk"
    integration = "www-integration.contractsfinder.service.gov.uk"
    production  = "www.contractsfinder.service.gov.uk"
  }

  cfs_secrets = {
    dev_email                             = "${local.cfs_secrets_arn}:DEV_EMAIL::"
    email_contactus                       = "${local.cfs_secrets_arn}:CONTACTUS_EMAIL::"
    email_subscription_authentication_key = "${local.cfs_secrets_arn}:EMAIL_SUBSCRIPTION_AUTHENTICATION_KEY::"
    email_subscription_cipher             = "${local.cfs_secrets_arn}:EMAIL_SUBSCRIPTION_CIPHER::"
    email_licence_request                 = "${local.cfs_secrets_arn}:LICENCEREQUEST_EMAIL::"
    google_analytics_key                  = "${local.cfs_secrets_arn}:GOOGLE_ANALYTICS_KEY::"
    google_tag_manager_key                = "${local.cfs_secrets_arn}:GOOGLE_TAG_MANAGER_KEY::"
    http_basic_auth_pass                  = "${local.cfs_secrets_arn}:HTTP_BASIC_AUTH_PASS::"
    http_basic_auth_user                  = "${local.cfs_secrets_arn}:HTTP_BASIC_AUTH_USER::"
    letsencrypt_key_authorization         = "${local.cfs_secrets_arn}:LETSENCRYPT_KEY_AUTHORIZATION::"
    letsencrypt_token                     = "${local.cfs_secrets_arn}:LETSENCRYPT_TOKEN::"
    run_guest_token                       = "${local.cfs_secrets_arn}:RUN_GUEST_TOKEN::"
    run_migrator_token                    = "${local.cfs_secrets_arn}:RUN_MIGRATOR_TOKEN::"
    run_registrar_token                   = "${local.cfs_secrets_arn}:RUN_REGISTRAR_TOKEN::"
    saml_entity_id                        = "${local.cfs_secrets_arn}:SAML_ENTITY_ID::"
    saml_slo_url                          = "${local.cfs_secrets_arn}:SAML_SLO_URL::"
    saml_sso_url                          = "${local.cfs_secrets_arn}:SAML_SSO_URL::"
    saml_x509cert                         = "${local.cfs_secrets_arn}:SAML_X509CERT::"
    srs_private_key                       = "${local.cfs_secrets_arn}:SRS_PRIVATE_KEY::"
    sso_api_authentication_key            = "${local.cfs_secrets_arn}:SSO_API_AUTHENTICATION_KEY::"
    sso_cipher                            = "${local.cfs_secrets_arn}:SSO_CIPHER::"
    sso_token                             = "${local.cfs_secrets_arn}:SSO_TOKEN::"
    sso_token_session_authentication_key  = "${local.cfs_secrets_arn}:SSO_TOKEN_SESSION_AUTHENTICATION_KEY::"
    sso_token_session_cipher              = "${local.cfs_secrets_arn}:SSO_TOKEN_SESSION_CIPHER::"
    email_tech_support                    = "${local.cfs_secrets_arn}:TECHSUPPORT_EMAIL::"
    email_user_research                   = "${local.cfs_secrets_arn}:USER_RESEARCH_EMAIL::"
  }

  cfs_parameters = {
    app_host_address                    = "%"
    buyer_corporate_identifier_prefixes = "sid4gov.cabinetoffice.gov.uk|supplierregistration.cabinetoffice.gov.uk"
    cookie_domain                       = local.cfs_site_domains[var.environment]
    data_harvester_folder_format        = ""
    db_host                             = var.db_cfs_cluster_address
    db_name                             = var.db_cfs_cluster_name
    db_port                             = 3306
    database_user_host_address          = "%"
    demo                                = false
    cfs_allowed_target_email_domains    = join(",", local.cfs_allowed_target_email_domains[var.environment])
    environment                         = upper(var.environment)
    http_basic_auth_enabled             = 0
    include_devel                       = false
    local_version                       = 1000
    site_domain                         = local.cfs_site_domains[var.environment]
    ssl_service                         = true
    email_support                       = var.is_production ? "noreply@contractsfinder.service.gov.uk" : "noreply@${var.public_domain}"
    valid_until                         = 1924990799
  }

  cfs_service_paremeters = {
    container_port  = var.service_configs.cfs.port
    cpu             = var.service_configs.cfs.cpu
    host_port       = var.service_configs.cfs.port
    image           = local.ecr_urls[var.service_configs.cfs.name]
    lg_name         = aws_cloudwatch_log_group.tasks[var.service_configs.cfs.name].name
    lg_prefix       = "app"
    lg_region       = data.aws_region.current.name
    memory          = var.service_configs.cfs.memory
    name            = var.service_configs.cfs.name
    public_domain   = var.public_domain
    service_version = local.service_version_cfs
    vpc_cidr        = var.vpc_cider
  }

  cfs_scheduler_service_paremeters = {
    container_port  = var.service_configs.cfs_scheduler.port
    cpu             = var.service_configs.cfs_scheduler.cpu
    host_port       = var.service_configs.cfs_scheduler.port
    image           = local.ecr_urls[var.service_configs.cfs_scheduler.name]
    lg_name         = aws_cloudwatch_log_group.tasks[var.service_configs.cfs_scheduler.name].name
    lg_prefix       = "app"
    lg_region       = data.aws_region.current.name
    memory          = var.service_configs.cfs_scheduler.memory
    name            = var.service_configs.cfs_scheduler.name
    public_domain   = var.public_domain
    service_version = local.service_version_cfs
    vpc_cidr        = var.vpc_cider
  }

  cfs_migrations_service_paremeters = {
    container_port  = var.service_configs.cfs_migrations.port
    cpu             = var.service_configs.cfs_migrations.cpu
    host_port       = var.service_configs.cfs_migrations.port
    image           = local.ecr_urls[var.service_configs.cfs_migrations.name]
    lg_name         = aws_cloudwatch_log_group.tasks[var.service_configs.cfs_migrations.name].name
    lg_prefix       = "app"
    lg_region       = data.aws_region.current.name
    memory          = var.service_configs.cfs_migrations.memory
    name            = var.service_configs.cfs_migrations.name
    public_domain   = var.public_domain
    service_version = local.service_version_cfs
    vpc_cidr        = var.vpc_cider
  }

  cfs_container_parameters = merge(
    local.cfs_parameters,
    local.cfs_service_paremeters,
    local.cfs_secrets
  )

  cfs_scheduler_container_parameters = merge(
    local.cfs_parameters,
    local.cfs_scheduler_service_paremeters,
    local.cfs_secrets
  )

  cfs_migrations_container_parameters = merge(
    local.cfs_parameters,
    local.cfs_migrations_service_paremeters,
    local.cfs_secrets
  )

  migrations_cfs = ["cfs-migrations"]

  migration_configs_cfs = {
    for name, config in var.service_configs :
    config.name => config if contains(local.migrations_cfs, config.name)
  }

  cfs_allowed_target_email_domains = {
    development = [
      "goaco.com"
    ]
    staging = [
      "goaco.com"
    ]
    integration = [
      "adb.co.uk",
      "atamis.co.uk",
      "axians.com",
      "b2bquote.co.uk",
      "bankofengland.co.uk",
      "bipsolutions.com",
      "bravosolution.com",
      "cabinetoffice.gov.uk",
      "caeser.org",
      "cimple.uk",
      "cognizant.com",
      "commercedecisions.com",
      "crowncommercial.gov.uk",
      "curtisfitchglobal.com",
      "dfid.gov.uk",
      "digital.cabinet-office.gov.uk",
      "eu-supply.com",
      "eurodyn.com",
      "evosysglobal.com",
      "excelerateds2p.com",
      "firefly-online.net",
      "fusionpractices.com",
      "geometrasystems.co.uk",
      "goaco.com",
      "govfsl.com",
      "gpa.gov.uk",
      "guinness.org.uk",
      "hillingdon.gov.uk",
      "homeoffice.gov.uk",
      "hull.ac.uk",
      "in-tend.co.uk",
      "incic.org.uk",
      "jaggaer.com",
      "londoncouncils.gov.uk",
      "maistro.com",
      "mastersoftware.co.uk",
      "millstream.eu",
      "mk9-development.com",
      "multiquote.com",
      "mytenders.co.uk",
      "ne1procurementservices.com",
      "nepo.org",
      "nitrous.city",
      "nqc.com",
      "nqcltd.com",
      "Olenick.com",
      "oneadvanced.com",
      "oracle.com",
      "panacea-software.com",
      "platformhg.com",
      "Proactis.com",
      "proactisinterfaces.com",
      "prologic.ie",
      "publiccontractsscotland.gov.uk",
      "sap.com",
      "sell2wales.gov.wales",
      "somerset.gov.uk",
      "sourcedogg.com",
      "spendnetwork.com",
      "supplychainpartner.com",
      "sussex.ac.uk",
      "tcs.com",
      "technologyonecorp.com",
      "tenderlake.com",
      "testpartners.co.uk",
      "trisaas.com",
      "useadam-tech.com",
      "useadam.co.uk",
      "viaem.co.uk",
      "waxdigital.com"
    ]
    production = []
  }

  cfs_service_allowed_origins = {
    development = []
    staging = [
      "https://cfs.staging.supplier-information.find-tender.service.gov.uk",
    ]
    integration = [
      "https://cfs.integration.supplier-information.find-tender.service.gov.uk",
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
      "https://cfs.supplier-information.find-tender.service.gov.uk",
      "https://www.find-tender.service.gov.uk"
    ]
  }

}
