locals {

  account_ids = {
    for name, env in local.environments : name => env.account_id
  }

  cidr_b_production           = 1
  cidr_b_staging              = 2
  cidr_b_development          = 3
  cidr_b_integration          = 4
  cidr_b_orchestrator         = 5
  cidr_b_external_integration = 6

  environment = get_env("TG_ENVIRONMENT", "development")

  is_production = contains(["production"], local.environment)

  environments = {
    orchestrator = {
      cidr_block             = "10.${local.cidr_b_orchestrator}.0.0/16"
      account_id             = 891377225335
      name                   = "orchestrator"
      postgres_instance_type = "db.t4g.micro"
      private_subnets = [
        "10.${local.cidr_b_orchestrator}.101.0/24",
        "10.${local.cidr_b_orchestrator}.102.0/24",
        "10.${local.cidr_b_orchestrator}.103.0/24"
      ]
      public_subnets = [
        "10.${local.cidr_b_orchestrator}.1.0/24",
        "10.${local.cidr_b_orchestrator}.2.0/24",
        "10.${local.cidr_b_orchestrator}.3.0/24"
      ]
      redis_node_type  = "cache.t2.micro"
      top_level_domain = "supplier.information.findatender.codatt.net"

      externals_cidr_block      = "integration account feature" # To be deprecated after FTS Migration
      externals_private_subnets = "integration account feature" # To be deprecated after FTS Migration
    }
    development = {
      cidr_block                        = "10.${local.cidr_b_development}.0.0/16"
      account_id                        = 471112892058
      canary_schedule_expression        = "rate(30 minutes)" # "cron(15 7,11,15 ? * MON-FRI)" # UTC+0
      cfs_allowed_target_email_domains  = ["goaco.com"]
      fts_allowed_target_email_domains  = ["goaco.com"]
      fts_azure_frontdoor               = null
      cfs_service_allowed_origins       = []
      fts_extra_domains                 = []
      fts_service_allowed_origins       = []
      mail_from_domains                 = []
      mysql_aurora_engine_version       = "5.7.mysql_aurora.2.12.5"
      mysql_aurora_family               = "aurora-mysql5.7"
      mysql_aurora_instance_type        = "db.r5.8xlarge"
      name                              = "dev"
      onelogin_logout_notification_urls = [
        "https://test-findtender.nqc.com/auth/backchannellogout"
      ]
      pinned_service_version_cfs        = null
      pinned_service_version_fts        = null
      pinned_service_version            = null
      postgres_instance_type            = "db.t4g.micro"
      postgres_aurora_instance_type     = "db.r5.large"
      private_subnets = [
        "10.${local.cidr_b_development}.101.0/24",
        "10.${local.cidr_b_development}.102.0/24",
        "10.${local.cidr_b_development}.103.0/24"
      ]
      public_subnets = [
        "10.${local.cidr_b_development}.1.0/24",
        "10.${local.cidr_b_development}.2.0/24",
        "10.${local.cidr_b_development}.3.0/24"
      ]
      redis_node_type  = "cache.t3.small"
      top_level_domain = "supplier-information.find-tender.service.gov.uk"

      externals_cidr_block      = "integration account feature" # To be deprecated after FTS Migration
      externals_private_subnets = "integration account feature" # To be deprecated after FTS Migration
    }
    staging = {
      cidr_block                        = "10.${local.cidr_b_staging}.0.0/16"
      account_id                        = 905418042182
      canary_schedule_expression        = "rate(30 minutes)"
      cfs_allowed_target_email_domains  = ["goaco.com"]
      cfs_extra_domains                 = ["www-preview.contractsfinder.service.gov.uk"]
      fts_allowed_target_email_domains  = ["goaco.com"]
      fts_extra_domains                 = ["www-staging.find-tender.service.gov.uk"]
      fts_azure_frontdoor               = null
      cfs_service_allowed_origins       = [
        "https://cfs.staging.supplier-information.find-tender.service.gov.uk",
      ]
      fts_service_allowed_origins       = [
        "https://fts.staging.supplier-information.find-tender.service.gov.uk",
      ]
      name                              = "staging"
      mail_from_domains                 = []
      mysql_aurora_engine_version       = "5.7.mysql_aurora.2.12.5"
      mysql_aurora_family               = "aurora-mysql5.7"
      mysql_aurora_instance_type        = "db.r5.8xlarge"
      onelogin_logout_notification_urls = [
        "https://www-staging.find-tender.service.gov.uk/auth/backchannellogout",
        "https://fts.staging.supplier-information.find-tender.service.gov.uk/auth/backchannellogout"
      ]
      pinned_service_version_cfs        = "1.0.4"
      pinned_service_version_fts        = "1.0.18"
      pinned_service_version            = "1.0.78"
      postgres_instance_type            = "db.t4g.micro"
      postgres_aurora_instance_type     = "db.r5.large"
      private_subnets = [
        "10.${local.cidr_b_staging}.101.0/24",
        "10.${local.cidr_b_staging}.102.0/24",
        "10.${local.cidr_b_staging}.103.0/24"
      ]
      public_subnets = [
        "10.${local.cidr_b_staging}.1.0/24",
        "10.${local.cidr_b_staging}.2.0/24",
        "10.${local.cidr_b_staging}.3.0/24"
      ]
      redis_node_type  = "cache.t3.medium"
      top_level_domain = "supplier-information.find-tender.service.gov.uk"

      externals_cidr_block      = "integration account feature" # To be deprecated after FTS Migration
      externals_private_subnets = "integration account feature" # To be deprecated after FTS Migration
    }
    integration = {
      cidr_block                        = "10.${local.cidr_b_integration}.0.0/16"
      account_id                        = 767397666448
      canary_schedule_expression        = "rate(30 minutes)"
      cfs_allowed_target_email_domains  = [
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
      cfs_extra_domains                 = ["www-integration.contractsfinder.service.gov.uk"]
      cfs_service_allowed_origins       = [
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
      fts_allowed_target_email_domains = [
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
        "jaggaer.com",
        "justice.gov.uk",
        "Kainos.com",
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
      fts_extra_domains                 = ["www-tpp.find-tender.service.gov.uk"]
      fts_azure_frontdoor               = null
      fts_service_allowed_origins       = [
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
      mail_from_domains                 = []
      mysql_aurora_engine_version       = "5.7.mysql_aurora.2.12.5"
      mysql_aurora_family               = "aurora-mysql5.7"
      mysql_aurora_instance_type        = "db.r5.8xlarge"
      name                              = "integration"
      onelogin_logout_notification_urls = [
        "https://truk-alpha.nqc.com/auth/backchannellogout",
        "https://truk-performance.nqc.com/auth/backchannellogout",
        "https://truk-prod.nqc.com/auth/backchannellogout",
        "https://www-tpp-preview.find-tender.service.gov.uk/auth/backchannellogout",
        "https://www-tpp.find-tender.service.gov.uk/auth/backchannellogout",
        "https://fts.integration.supplier-information.find-tender.service.gov.uk/auth/backchannellogout"
      ]
      pinned_service_version_cfs        = "1.0.4"
      pinned_service_version_fts        = "1.0.15"
      pinned_service_version            = "1.0.78"
      postgres_instance_type            = "db.t4g.micro"
      postgres_aurora_instance_type     = "db.r5.large"
      private_subnets = [
        "10.${local.cidr_b_integration}.101.0/24",
        "10.${local.cidr_b_integration}.102.0/24",
        "10.${local.cidr_b_integration}.103.0/24"
      ]
      public_subnets = [
        "10.${local.cidr_b_integration}.1.0/24",
        "10.${local.cidr_b_integration}.2.0/24",
        "10.${local.cidr_b_integration}.3.0/24"
      ]
      redis_node_type  = "cache.t3.medium"
      top_level_domain = "supplier-information.find-tender.service.gov.uk"

      externals_cidr_block      = "10.${local.cidr_b_external_integration}.0.0/16"
      externals_private_subnets = [
        "10.${local.cidr_b_external_integration}.101.0/24",
        "10.${local.cidr_b_external_integration}.102.0/24",
        "10.${local.cidr_b_external_integration}.103.0/24"
      ]
    }
    production = {
      cidr_block                        = "10.${local.cidr_b_production}.0.0/16"
      account_id                        = 471112843276
      canary_schedule_expression        = "rate(15 minutes)"
      cfs_allowed_target_email_domains  = []
      cfs_extra_domains                 = ["www.contractsfinder.service.gov.uk"]
      fts_extra_domains                 = []
      cfs_service_allowed_origins       = [
        "https://cfs.supplier-information.find-tender.service.gov.uk",
        "https://www.find-tender.service.gov.uk"
      ]
      fts_allowed_target_email_domains  = []
      fts_extra_domains                 = ["www.find-tender.service.gov.uk", "find-tender.service.gov.uk"]
      fts_azure_frontdoor               = "nqc-front-door-uksouth.azurefd.net"
      fts_service_allowed_origins       = [
        "https://fts.supplier-information.find-tender.service.gov.uk",
        "https://www.find-tender.service.gov.uk"
      ]
      mail_from_domains                 = ["find-tender.service.gov.uk", "contractsfinder.service.gov.uk"]
      mysql_aurora_engine_version       = "5.7.mysql_aurora.2.12.5"
      mysql_aurora_family               = "aurora-mysql5.7"
      mysql_aurora_instance_type        = "db.r5.8xlarge"
      name                              = "production"
      onelogin_logout_notification_urls = [
        "https://www.find-tender.service.gov.uk/auth/backchannellogout",
        "https://fts.supplier-information.find-tender.service.gov.uk/auth/backchannellogout"
      ],
      pinned_service_version_cfs        = "1.0.4"
      pinned_service_version_fts        = "1.0.15"
      pinned_service_version            = "1.0.78"
      postgres_instance_type            = "db.t4g.micro"
      postgres_aurora_instance_type     = "db.r5.8xlarge"
      postgres_aurora_instance_type_ev  = "db.r5.4xlarge"
      private_subnets = [
        "10.${local.cidr_b_production}.101.0/24",
        "10.${local.cidr_b_production}.102.0/24",
        "10.${local.cidr_b_production}.103.0/24"
      ]
      public_subnets = [
        "10.${local.cidr_b_production}.1.0/24",
        "10.${local.cidr_b_production}.2.0/24",
        "10.${local.cidr_b_production}.3.0/24"
      ]
      redis_node_type  = "cache.r5.xlarge"
      top_level_domain = "supplier-information.find-tender.service.gov.uk"

      externals_cidr_block      = "integration account feature" # To be deprecated after FTS Migration
      externals_private_subnets = "integration account feature" # To be deprecated after FTS Migration
    }
  }

  aurora_mysql_engine_version       = try(local.environments[local.environment].mysql_aurora_engine_version, "8.0.mysql_aurora.3.07.1")
  aurora_mysql_family               = try(local.environments[local.environment].mysql_aurora_family, "aurora-mysql8.0")
  aurora_mysql_instance_type        = try(local.environments[local.environment].mysql_aurora_instance_type, local.aurora_postgres_instance_type)
  aurora_postgres_instance_type     = try(local.environments[local.environment].postgres_aurora_instance_type, null)
  aurora_postgres_instance_type_ev  = try(local.environments[local.environment].postgres_aurora_instance_type_ev, local.aurora_postgres_instance_type)
  cfs_allowed_target_email_domains  = try(local.environments[local.environment].cfs_allowed_target_email_domains, null)
  cfs_extra_domains                 = try(local.environments[local.environment].cfs_extra_domains, [])
  cfs_service_allowed_origins       = try(local.environments[local.environment].cfs_service_allowed_origins, null)
  fts_allowed_target_email_domains  = try(local.environments[local.environment].fts_allowed_target_email_domains, null)
  fts_extra_domains                 = try(local.environments[local.environment].fts_extra_domains, [])
  fts_azure_frontdoor               = try(local.environments[local.environment].fts_azure_frontdoor, null)
  fts_service_allowed_origins       = try(local.environments[local.environment].fts_service_allowed_origins, null)
  mail_from_domains                  = try(local.environments[local.environment].mail_from_domains, [])
  onelogin_logout_notification_urls = try(local.environments[local.environment].onelogin_logout_notification_urls, null)
  pinned_service_version            = try(local.environments[local.environment].pinned_service_version, null)
  pinned_service_version_cfs        = try(local.environments[local.environment].pinned_service_version_cfs, null)
  pinned_service_version_fts        = try(local.environments[local.environment].pinned_service_version_fts, null)
  redis_node_type                   = try(local.environments[local.environment].redis_node_type, null)

  product = {
    name               = "CDP SIRSI"
    resource_name      = "cdp-sirsi"
    public_hosted_zone = local.environment == "production" ? local.environments[local.environment].top_level_domain : "${local.environments[local.environment].name}.${local.environments[local.environment].top_level_domain}"
  }

  external_product = {
    name          = "CDP FTS"
    resource_name = "cdp-sirsi-ext-fts"
    mysql_access_allowed_ip_ranges = ["0.0.0.0/0"]
  }

  service_configs_base = {
    authority                            = {}
    av_scanner_app                       = {}
    cfs                                  = { desired_count = 3, cpu = 4096,  memory = 8192}
    cfs_migrations                       = { desired_count = 1 }
    cfs_scheduler                        = { desired_count = 1 }
    commercial_tools_app                 = {}
    commercial_tools_api                 = {}
    data_sharing                         = {}
    entity_verification                  = {}
    entity_verification_migrations       = { cpu = 256,  memory = 512}
    forms                                = {}
    fts                                  = { desired_count = 3, cpu = 4096,  memory = 8192}
    fts_healthcheck                      = { desired_count = 1 }
    fts_migrations                       = { desired_count = 1 }
    fts_scheduler                        = { desired_count = 1, cpu = 4096,  memory = 8192 }
    organisation                         = {}
    organisation_app                     = {}
    organisation_information_migrations  = { cpu = 256,  memory = 512}
    outbox_processor_entity_verification = { desired_count = 1 }
    outbox_processor_organisation        = { desired_count = 1 }
    person                               = {}
    scheduled_worker                     = { desired_count = 1 }
    tenant                               = {}
  }

  desired_counts = {
    orchestrator = 0
    development  = 2
    integration  = 2
    staging      = 2
    production   = 6
  }

  resource_defaults = {
    development  = { cpu = 256,  memory = 512  }
    orchestrator = { cpu = 256,  memory = 512  }
    integration  = { cpu = 512,  memory = 1024 }
    staging      = { cpu = 512,  memory = 1024 }
    production   = { cpu = 1024, memory = 2048 }
  }

  service_configs_scaling = {
    for service, config in local.service_configs_base :
    service => merge(
      local.resource_defaults[local.environment],
      { desired_count = local.desired_counts[local.environment] },
      config
    )
  }

  service_configs_common = {
    authority                            = { port = 8092, port_host = 8092, name = "authority"}
    av_scanner_app                       = { port = 8095, port_host = 8095, name = "av-scanner-app"}
    cfs                                  = { port = 8060, port_host = 8060, name = "cfs"}
    cfs_migrations                       = { port = 8062, port_host = null, name = "cfs-migrations"}
    cfs_scheduler                        = { port = 8064, port_host = null, name = "cfs-scheduler"}
    commercial_tools_app                 = { port = 8192, port_host = 8192, name = "commercial-tools-app"}
    commercial_tools_api                 = { port = 8184, port_host = 8184, name = "commercial-tools-api"}
    data_sharing                         = { port = 8088, port_host = 8088, name = "data-sharing"}
    entity_verification                  = { port = 8094, port_host = 8094, name = "entity-verification"}
    entity_verification_migrations       = { port = 9191, port_host = null, name = "entity-verification-migrations"}
    forms                                = { port = 8086, port_host = 8086, name = "forms"}
    fts                                  = { port = 8070, port_host = 8070, name = "fts"}
    fts_healthcheck                      = { port = 8071, port_host = 8071, name = "fts-healthcheck"}
    fts_migrations                       = { port = 8072, port_host = null, name = "fts-migrations"}
    fts_scheduler                        = { port = 8074, port_host = null, name = "fts-scheduler"}
    organisation                         = { port = 8082, port_host = 8082, name = "organisation"}
    organisation_app                     = { port = 8090, port_host = 80  , name = "organisation-app"}
    organisation_information_migrations  = { port = 9090, port_host = null, name = "organisation-information-migrations"}
    outbox_processor_entity_verification = { port = 9096, port_host = 9096, name = "outbox-processor-entity-verification"}
    outbox_processor_organisation        = { port = 9098, port_host = 9098, name = "outbox-processor-organisation"}
    person                               = { port = 8084, port_host = 8084, name = "person" }
    scheduled_worker                     = { port = 9094, port_host = 9094, name = "scheduled-worker"}
    tenant                               = { port = 8080, port_host = 8080, name = "tenant" }
  }

  service_configs = {
    for key, value in local.service_configs_scaling :
    key => merge(value, local.service_configs_common[key])
  }

  tags = {
    environment = local.environment
    managed_by  = "terragrunt"
  }

  tools_configs = {
    clamav = {
      cpu       = 1024
      memory    = 3072
      name      = "clamav"
      port      = 9001
      port_host = 9001
    }
    clamav_rest = {
      cpu       = 1024
      memory    = 3072
      name      = "clamav-rest"
      port      = 9000
      port_host = 9000
    }
    cloud_beaver = {
      cpu       = 1024
      memory    = 3072
      name      = "cloud-beaver"
      port      = 8978
      port_host = 8978
    }
    grafana = {
      cpu       = 1024
      memory    = 3072
      name      = "grafana"
      port      = 3000
      port_host = 3000
    }
    healthcheck = {
      cpu       = 256
      memory    = 512
      name      = "healthcheck"
      port      = 3030
      port_host = 3030
    }
    k6 = {
      cpu       = 1024
      memory    = 3072
      name      = "k6"
      port      = 4040
      port_host = null
    }
    pgadmin = {
      cpu       = 256
      memory    = 512
      name      = "pgadmin"
      port      = 5050
      port_host = 5050
    }
  }

  tg = {
    state_bucket = "tfstate-${local.product.resource_name}-${local.environment}-${get_aws_account_id()}"
    state_key    = "${path_relative_to_include()}/terraform.tfstate"
  }

  versions = {
    postgres_engine = "16.3"
  }

}

remote_state {
  backend = "s3"
  config = {
    bucket                = local.tg.state_bucket
    disable_bucket_update = true
    dynamodb_table        = "terraform-locks"
    encrypt               = true
    key                   = local.tg.state_key
    region                = "eu-west-2"
  }
  generate = {
    path      = "remote_state.tf"
    if_exists = "overwrite"
  }
}

generate provider {
  path      = "temp_providers.tf"
  if_exists = "overwrite_terragrunt"
  contents = file("../providers.tf")
}

inputs = {
  environment                   = local.environment
  externals_product             = local.external_product
  externals_vpc_cidr            = local.environments[local.environment].externals_cidr_block
  externals_vpc_private_subnets = local.environments[local.environment].externals_private_subnets
  is_production                 = local.is_production
  postgres_engine_version       = local.versions.postgres_engine
  postgres_instance_type        = local.environments[local.environment].postgres_instance_type
  product                       = local.product
  tags                          = local.tags
  vpc_cidr                      = local.environments[local.environment].cidr_block
  vpc_private_subnets           = local.environments[local.environment].private_subnets
  vpc_public_subnets            = local.environments[local.environment].public_subnets
}

terraform {
  extra_arguments disable_input {
    commands = get_terraform_commands_that_need_input()
    arguments = ["-input=false"]
  }
}
