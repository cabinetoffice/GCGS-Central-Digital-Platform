locals {

  account_ids = {
    for name, env in local.environments : name => env.account_id
  }

  cidr_b_production   = 1
  cidr_b_staging      = 2
  cidr_b_development  = 3
  cidr_b_integration  = 4
  cidr_b_orchestrator = 5

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
      redis_node_type = "cache.t2.micro"
    }

    development = {
      cidr_block                  = "10.${local.cidr_b_development}.0.0/16"
      account_id                  = 471112892058
      canary_schedule_expression  = "rate(30 minutes)" # "cron(15 7,11,15 ? * MON-FRI)" # UTC+0
      cfs_extra_domains           = ["www-development.contractsfinder.service.gov.uk"]
      fts_apply_master_password   = false
      fts_extra_domains           = ["www-development.find-tender.service.gov.uk"]
      fts_restore_from_snapshot   = false
      fts_snapshot_identifier     = null
      mail_from_domains           = []
      mysql_aurora_engine_version = "5.7.mysql_aurora.2.12.5"
      mysql_aurora_family         = "aurora-mysql5.7"
      mysql_aurora_instance_type  = "db.r5.large"
      name                        = "dev"
      onelogin_logout_notification_urls = [
        "https://test-findtender.nqc.com/auth/backchannellogout"
      ]
      opensearch_availability_zone_count  = 2
      opensearch_dedicated_master_count   = 3
      opensearch_dedicated_master_enabled = true
      opensearch_dedicated_master_type    = "t3.small.search"
      opensearch_instance_count           = 2
      opensearch_instance_type            = "t3.small.search"
      pinned_service_version_cfs          = null
      pinned_service_version_fts          = null
      pinned_service_version              = null
      postgres_instance_type              = "db.t4g.micro"
      postgres_aurora_instance_type       = "db.r5.large"
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
      redis_node_type = "cache.t3.small"
    }

    staging = {
      cidr_block                  = "10.${local.cidr_b_staging}.0.0/16"
      account_id                  = 905418042182
      canary_schedule_expression  = "rate(30 minutes)"
      cfs_extra_domains           = ["www-preview.contractsfinder.service.gov.uk"]
      fts_apply_master_password   = false
      fts_extra_domains           = ["www-staging.find-tender.service.gov.uk"]
      fts_restore_from_snapshot   = false
      fts_snapshot_identifier     = null
      name                        = "staging"
      mail_from_domains           = []
      mysql_aurora_engine_version = "5.7.mysql_aurora.2.12.5"
      mysql_aurora_family         = "aurora-mysql5.7"
      mysql_aurora_instance_type  = "db.r5.xlarge"
      onelogin_logout_notification_urls = [
        "https://www-staging.find-tender.service.gov.uk/auth/backchannellogout",
        "https://fts.staging.supplier-information.find-tender.service.gov.uk/auth/backchannellogout"
      ]
      opensearch_availability_zone_count  = 2
      opensearch_dedicated_master_count   = 3
      opensearch_dedicated_master_enabled = true
      opensearch_dedicated_master_type    = "t3.small.search"
      opensearch_instance_count           = 2
      opensearch_instance_type            = "t3.medium.search"
      pinned_service_version_cfs          = "1.0.7"
      pinned_service_version_fts          = "1.3.1"
      pinned_service_version              = "1.0.83"
      postgres_instance_type              = "db.t4g.micro"
      postgres_aurora_instance_type       = "db.r5.large"
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
      redis_node_type = "cache.t3.medium"
    }

    integration = {
      cidr_block                  = "10.${local.cidr_b_integration}.0.0/16"
      account_id                  = 767397666448
      canary_schedule_expression  = "rate(30 minutes)"
      cfs_extra_domains           = ["www-integration.contractsfinder.service.gov.uk"]
      fts_apply_master_password   = false
      fts_extra_domains           = ["www-tpp.find-tender.service.gov.uk"]
      fts_restore_from_snapshot   = false
      fts_snapshot_identifier     = null
      mail_from_domains           = []
      mysql_aurora_engine_version = "5.7.mysql_aurora.2.12.5"
      mysql_aurora_family         = "aurora-mysql5.7"
      mysql_aurora_instance_type  = "db.r5.xlarge"
      name                        = "integration"
      onelogin_logout_notification_urls = [
        "https://truk-alpha.nqc.com/auth/backchannellogout",
        "https://truk-performance.nqc.com/auth/backchannellogout",
        "https://truk-prod.nqc.com/auth/backchannellogout",
        "https://www-tpp-preview.find-tender.service.gov.uk/auth/backchannellogout",
        "https://www-tpp.find-tender.service.gov.uk/auth/backchannellogout",
        "https://fts.integration.supplier-information.find-tender.service.gov.uk/auth/backchannellogout"
      ]
      opensearch_availability_zone_count  = 2
      opensearch_dedicated_master_count   = 3
      opensearch_dedicated_master_enabled = true
      opensearch_dedicated_master_type    = "t3.small.search"
      opensearch_instance_count           = 2
      opensearch_instance_type            = "t3.medium.search"
      pinned_service_version_cfs          = "1.0.7"
      pinned_service_version_fts          = "1.3.1"
      pinned_service_version              = "1.0.82"
      postgres_instance_type              = "db.t4g.micro"
      postgres_aurora_instance_type       = "db.r5.large"
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
      redis_node_type = "cache.t3.medium"
    }

    production = {
      cidr_block                  = "10.${local.cidr_b_production}.0.0/16"
      account_id                  = 471112843276
      canary_schedule_expression  = "rate(15 minutes)"
      cfs_extra_domains           = ["www.contractsfinder.service.gov.uk"]
      fts_apply_master_password   = false
      fts_extra_domains           = ["www.find-tender.service.gov.uk", "find-tender.service.gov.uk"]
      fts_restore_from_snapshot   = false
      fts_snapshot_identifier     = null
      mail_from_domains           = ["find-tender.service.gov.uk", "contractsfinder.service.gov.uk"]
      mysql_aurora_engine_version = "5.7.mysql_aurora.2.12.5"
      mysql_aurora_family         = "aurora-mysql5.7"
      mysql_aurora_instance_type  = "db.r5.8xlarge"
      name                        = "production"
      onelogin_logout_notification_urls = [
        "https://www.find-tender.service.gov.uk/auth/backchannellogout",
        "https://fts.supplier-information.find-tender.service.gov.uk/auth/backchannellogout"
      ],
      opensearch_availability_zone_count  = 3
      opensearch_dedicated_master_count   = 3
      opensearch_dedicated_master_enabled = true
      opensearch_dedicated_master_type    = "m6g.large.search"
      opensearch_instance_count           = 3
      opensearch_instance_type            = "m6g.large.search"
      pinned_service_version_cfs          = "1.0.7"
      pinned_service_version_fts          = "1.3.1"
      pinned_service_version              = "1.0.82"
      postgres_instance_type              = "db.t4g.micro"
      postgres_aurora_instance_type       = "db.r5.8xlarge"
      postgres_aurora_instance_type_ev    = "db.r5.4xlarge"
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
      redis_node_type = "cache.r5.xlarge"
    }
  }

  aurora_mysql_engine_version         = try(local.environments[local.environment].mysql_aurora_engine_version, "8.0.mysql_aurora.3.07.1")
  aurora_mysql_family                 = try(local.environments[local.environment].mysql_aurora_family, "aurora-mysql8.0")
  aurora_mysql_instance_type          = try(local.environments[local.environment].mysql_aurora_instance_type, local.aurora_postgres_instance_type)
  aurora_postgres_instance_type       = try(local.environments[local.environment].postgres_aurora_instance_type, null)
  aurora_postgres_instance_type_ev    = try(local.environments[local.environment].postgres_aurora_instance_type_ev, local.aurora_postgres_instance_type)
  cfs_extra_domains                   = try(local.environments[local.environment].cfs_extra_domains, [])
  fts_apply_master_password           = try(local.environments[local.environment].fts_apply_master_password, false)
  fts_extra_domains                   = try(local.environments[local.environment].fts_extra_domains, [])
  fts_restore_from_snapshot           = try(local.environments[local.environment].fts_restore_from_snapshot, false)
  fts_snapshot_identifier             = try(local.environments[local.environment].fts_snapshot_identifier, null)
  mail_from_domains                   = try(local.environments[local.environment].mail_from_domains, [])
  onelogin_logout_notification_urls   = try(local.environments[local.environment].onelogin_logout_notification_urls, null)
  opensearch_availability_zone_count  = try(local.environments[local.environment].opensearch_availability_zone_count, 2)
  opensearch_dedicated_master_count   = try(local.environments[local.environment].opensearch_dedicated_master_count, 3)
  opensearch_dedicated_master_enabled = try(local.environments[local.environment].opensearch_dedicated_master_enabled, true)
  opensearch_dedicated_master_type    = try(local.environments[local.environment].opensearch_dedicated_master_type, "t3.small.search")
  opensearch_instance_count           = try(local.environments[local.environment].opensearch_instance_count, 2)
  opensearch_instance_type            = try(local.environments[local.environment].opensearch_instance_type, "t3.small.search")
  pinned_service_version              = try(local.environments[local.environment].pinned_service_version, null)
  pinned_service_version_cfs          = try(local.environments[local.environment].pinned_service_version_cfs, null)
  pinned_service_version_fts          = try(local.environments[local.environment].pinned_service_version_fts, null)
  redis_node_type                     = try(local.environments[local.environment].redis_node_type, null)

  top_level_domain = "supplier-information.find-tender.service.gov.uk"

  product = {
    name               = "CDP SIRSI"
    resource_name      = "cdp-sirsi"
    public_hosted_zone = local.environment == "production" ? local.top_level_domain : "${local.environments[local.environment].name}.${local.top_level_domain}"
  }

  external_product = {
    name                           = "CDP FTS"
    resource_name                  = "cdp-sirsi-ext-fts"
    mysql_access_allowed_ip_ranges = ["0.0.0.0/0"]
  }

  service_configs_base = {
    authority                                     = {}
    av_scanner_app                                = {}
    cfs                                           = { desired_count = 3, cpu = 4096, memory = 8192 }
    cfs_migrations                                = { desired_count = 1 }
    cfs_scheduler                                 = { desired_count = 1 }
    commercial_tools_app                          = {}
    commercial_tools_api                          = {}
    commercial_tools_migrations                   = { cpu = 256, memory = 512 }
    data_sharing                                  = {}
    entity_verification                           = {}
    entity_verification_migrations                = { cpu = 256, memory = 512 }
    forms                                         = {}
    fts                                           = { desired_count = 3, cpu = 4096, memory = 8192 }
    fts_app                                       = { desired_count = 2 }
    fts_healthcheck                               = { desired_count = 0 }
    fts_migrations                                = { desired_count = 1 }
    fts_scheduler                                 = { desired_count = 1, cpu = 4096, memory = 8192 }
    fts_search_api                                = { desired_count = 2 }
    fts_search_indexer                            = { desired_count = 1 }
    organisation                                  = {}
    organisation_app                              = {}
    organisation_information_migrations           = { cpu = 256, memory = 512 }
    outbox_processor_entity_verification          = { desired_count = 1 }
    outbox_processor_organisation                 = { desired_count = 1 }
    person                                        = {}
    scheduled_worker                              = { desired_count = 1 }
    tenant                                        = {}
    user_management_api                           = { desired_count = local.environment == "development" ? 1 : 0 }
    user_management_app                           = { desired_count = local.environment == "development" ? 1 : 0 }
    user_management_migrations                    = { cpu = 256, memory = 512 }
  }

  desired_counts = {
    orchestrator = 0
    development  = 2
    integration  = 2
    staging      = 2
    production   = 6
  }

  resource_defaults = {
    development  = { cpu = 256, memory = 512 }
    orchestrator = { cpu = 256, memory = 512 }
    integration  = { cpu = 512, memory = 1024 }
    staging      = { cpu = 512, memory = 1024 }
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
    authority                                     = { cluster = "sirsi",     type = "web-service",  listener_priority = 118, name = "authority" }
    av_scanner_app                                = { cluster = "sirsi",     type = "web-service",  listener_priority = 112, name = "av-scanner-app" }
    cfs                                           = { cluster = "sirsi-php", type = "web-service",  listener_priority = 310, name = "cfs" }
    cfs_migrations                                = { cluster = "sirsi-php", type = "db-migration",                          name = "cfs-migrations" }
    cfs_scheduler                                 = { cluster = "sirsi-php", type = "service",                               name = "cfs-scheduler" }
    commercial_tools_api                          = { cluster = "sirsi",     type = "web-service",  listener_priority = 113, name = "commercial-tools-api" }
    commercial_tools_app                          = { cluster = "sirsi",     type = "web-service",  listener_priority = 111, name = "commercial-tools-app" }
    commercial_tools_migrations                   = { cluster = "sirsi",     type = "db-migration",                          name = "commercial-tools-migrations" }
    data_sharing                                  = { cluster = "sirsi",     type = "web-service",  listener_priority = 114, name = "data-sharing" }
    entity_verification                           = { cluster = "sirsi",     type = "web-service",  listener_priority = 115, name = "entity-verification" }
    entity_verification_migrations                = { cluster = "sirsi",     type = "db-migration",                          name = "entity-verification-migrations" }
    forms                                         = { cluster = "sirsi",     type = "web-service",  listener_priority = 116, name = "forms" }
    fts                                           = { cluster = "sirsi-php", type = "web-service",  listener_priority = 311, name = "fts" }
    fts_app                                       = { cluster = "fts",       type = "service",      listener_priority = 210, name = "fts-app" }
    fts_healthcheck                               = { cluster = "sirsi-php", type = "web-service",  listener_priority = 312, name = "fts-healthcheck" }
    fts_migrations                                = { cluster = "sirsi-php", type = "db-migration",                          name = "fts-migrations" }
    fts_scheduler                                 = { cluster = "sirsi-php", type = "service",                               name = "fts-scheduler" }
    fts_search_api                                = { cluster = "fts",       type = "service",      listener_priority = 211, name = "fts-search-api" }
    fts_search_indexer                            = { cluster = "fts",       type = "service",                               name = "fts-search-indexer" }
    organisation                                  = { cluster = "sirsi",     type = "web-service",  listener_priority = 117, name = "organisation" }
    organisation_app                              = { cluster = "sirsi",     type = "web-service",  listener_priority = 110, name = "organisation-app" }
    organisation_information_migrations           = { cluster = "sirsi",     type = "db-migration",                          name = "organisation-information-migrations" }
    outbox_processor_entity_verification          = { cluster = "sirsi",     type = "service",      listener_priority = 119, name = "outbox-processor-entity-verification" }
    outbox_processor_organisation                 = { cluster = "sirsi",     type = "service",      listener_priority = 120, name = "outbox-processor-organisation" }
    person                                        = { cluster = "sirsi",     type = "web-service",  listener_priority = 121, name = "person" }
    scheduled_worker                              = { cluster = "sirsi",     type = "service",      listener_priority = 122, name = "scheduled-worker" }
    tenant                                        = { cluster = "sirsi",     type = "web-service",  listener_priority = 123, name = "tenant" }
    user_management_api                           = { cluster = "sirsi",     type = "web-service",  listener_priority = 130, name = "user-management-api" }
    user_management_app                           = { cluster = "sirsi",     type = "web-service",  listener_priority = 131, name = "user-management-app" }
    user_management_migrations                    = { cluster = "sirsi",     type = "db-migration", name = "user-management-migrations" }
  }

  service_configs = {
    for key, value in local.service_configs_scaling :
    key => merge(value, local.service_configs_common[key])
  }

  tags = {
    environment = local.environment
    managed_by  = "terragrunt"
  }

  tools_configs_common = {
    clamav = {
      cpu    = 1024
      memory = 3072
      name   = "clamav"
      port   = 9001
    }
    clamav_rest = {
      cpu    = 1024
      memory = 3072
      name   = "clamav-rest"
      port   = 9000
    }
    cloud_beaver = {
      cpu    = 1024
      memory = 3072
      name   = "cloud-beaver"
      port   = 8978
    }
    grafana = {
      cpu    = 1024
      memory = 3072
      name   = "grafana"
      port   = 3000
    }
    healthcheck = {
      cpu    = 256
      memory = 512
      name   = "healthcheck"
      port   = 3030
    }
    k6 = {
      name = "k6"
      port = 4040
    }
    opensearch_admin = {
      name = "opensearch-admin"
      port = 5601
    }
    opensearch_gateway = {
      name = "opensearch-gateway"
      port = 5602
    }
    opensearch_debugtask = {
      name = "opensearch-debugtask"
      port = 5603
    }
    s3_uploader = {
      name = "s3-uploader"
      port = 8000
    }
  }

  tools_configs = {
    for service, config in local.tools_configs_common :
    service => merge(
      local.resource_defaults[local.environment],
      config
    )
  }

  tg = {
    state_bucket = "tfstate-${local.product.resource_name}-${local.environment}-${get_aws_account_id()}"
    state_key    = "${path_relative_to_include()}/terraform.tfstate"
  }

  versions = {
    postgres_engine = "16.8"
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
  contents  = file("../providers.tf")
}

inputs = {
  aurora_postgres_engine_version      = local.versions.postgres_engine
  environment                         = local.environment
  externals_product                   = local.external_product
  is_production                       = local.is_production
  opensearch_availability_zone_count  = local.opensearch_availability_zone_count
  opensearch_dedicated_master_count   = local.opensearch_dedicated_master_count
  opensearch_dedicated_master_enabled = local.opensearch_dedicated_master_enabled
  opensearch_dedicated_master_type    = local.opensearch_dedicated_master_type
  opensearch_instance_count           = local.opensearch_instance_count
  opensearch_instance_type            = local.opensearch_instance_type
  postgres_instance_type              = local.environments[local.environment].postgres_instance_type
  product                             = local.product
  tags                                = local.tags
  vpc_cidr                            = local.environments[local.environment].cidr_block
  vpc_private_subnets                 = local.environments[local.environment].private_subnets
  vpc_public_subnets                  = local.environments[local.environment].public_subnets
}

terraform {
  extra_arguments disable_input {
    commands  = get_terraform_commands_that_need_input()
    arguments = ["-input=false"]
  }
}
