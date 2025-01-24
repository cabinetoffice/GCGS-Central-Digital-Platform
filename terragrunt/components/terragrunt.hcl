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
      top_level_domain = "findatender.codatt.net"

      externals_cidr_block      = "integration account feature" # To be deprecated after FTS Migration
      externals_private_subnets = "integration account feature" # To be deprecated after FTS Migration
    }
    development = {
      cidr_block                        = "10.${local.cidr_b_development}.0.0/16"
      account_id                        = 471112892058
      canary_schedule_expression        = "rate(30 minutes)" # "cron(15 7,11,15 ? * MON-FRI)" # UTC+0
      fts_azure_frontdoor               = null
      name                              = "dev"
      onelogin_logout_notification_urls = [
        "https://test-findtender.nqc.com/auth/backchannellogout",
        "https://stanvolcere.nqc.com/auth/backchannellogout",
        "https://nadeemshafi2.nqc.com/auth/backchannellogout"
      ]
      pinned_service_version            = null
      postgres_instance_type            = "db.t4g.micro"
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
      top_level_domain = "findatender.codatt.net"

      externals_cidr_block      = "integration account feature" # To be deprecated after FTS Migration
      externals_private_subnets = "integration account feature" # To be deprecated after FTS Migration
    }
    staging = {
      cidr_block                        = "10.${local.cidr_b_staging}.0.0/16"
      account_id                        = 905418042182
      canary_schedule_expression        = "rate(30 minutes)"
      fts_azure_frontdoor               = null
      name                              = "staging"
      onelogin_logout_notification_urls = ["https://sirsi-integration-findtender.nqc.com/auth/backchannellogout"]
      pinned_service_version            = "1.0.25"
      postgres_instance_type            = "db.t4g.micro"
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
      top_level_domain = "findatender.codatt.net"

      externals_cidr_block      = "integration account feature" # To be deprecated after FTS Migration
      externals_private_subnets = "integration account feature" # To be deprecated after FTS Migration
    }
    integration = {
      cidr_block                        = "10.${local.cidr_b_integration}.0.0/16"
      account_id                        = 767397666448
      canary_schedule_expression        = "rate(30 minutes)"
      fts_azure_frontdoor               = null
      name                              = "integration"
      onelogin_logout_notification_urls = ["https://test-findtender.nqc.com/auth/backchannellogout"]
      pinned_service_version            = "1.0.24"
      postgres_instance_type            = "db.t4g.micro"
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
      top_level_domain = "findatender.codatt.net"

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
      fts_azure_frontdoor               = "nqc-front-door-uksouth.azurefd.net"
      name                              = "production"
      onelogin_logout_notification_urls = ["https://www.private-beta.find-tender.service.gov.uk/auth/backchannellogout"]
      pinned_service_version            = "1.0.25"
      postgres_instance_type            = "db.t4g.micro"
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
      top_level_domain = "find-tender.service.gov.uk"

      externals_cidr_block      = "integration account feature" # To be deprecated after FTS Migration
      externals_private_subnets = "integration account feature" # To be deprecated after FTS Migration
    }
  }

  fts_azure_frontdoor               = try(local.environments[local.environment].fts_azure_frontdoor, null)
  onelogin_logout_notification_urls = try(local.environments[local.environment].onelogin_logout_notification_urls, null)
  pinned_service_version            = try(local.environments[local.environment].pinned_service_version, null)

  product = {
    name               = "CDP SIRSI"
    resource_name      = "cdp-sirsi"
    public_hosted_zone = local.environment == "production" ? local.environments[local.environment].top_level_domain : "${local.environments[local.environment].name}.supplier.information.${local.environments[local.environment].top_level_domain}"
  }

  external_product = {
    name          = "CDP FTS"
    resource_name = "cdp-sirsi-ext-fts"
    mysql_access_allowed_ip_ranges = ["0.0.0.0/0"]
  }

  desired_count_non_production = 1
  desired_count_production     = 1
  frontend_desired_count_non_production = 4

  service_configs_scaling_non_production = {
    authority = {
      cpu           = 256
      desired_count = local.desired_count_non_production
      memory        = 512
    }
    av_scanner_app = {
      cpu           = 256
      desired_count = local.desired_count_non_production
      memory        = 512
    }
    data_sharing = {
      cpu           = 256
      desired_count = local.desired_count_non_production
      memory        = 512
    }
    entity_verification = {
      cpu           = 256
      desired_count = local.desired_count_non_production
      memory        = 512
    }
    entity_verification_migrations = {
      cpu           = 256
      desired_count = local.desired_count_non_production
      memory        = 512
    }
    forms = {
      cpu           = 256
      desired_count = local.desired_count_non_production
      memory        = 512
    }
    organisation = {
      cpu           = 256
      desired_count = local.desired_count_non_production
      memory        = 512
    }
    organisation_app = {
      cpu           = 256
      desired_count = local.frontend_desired_count_non_production
      memory        = 512
    }
    organisation_information_migrations = {
      cpu           = 256
      desired_count = local.desired_count_non_production
      memory        = 512
    }
    person = {
      cpu           = 256
      desired_count = local.desired_count_non_production
      memory        = 512
    }
    tenant = {
      cpu           = 256
      desired_count = local.desired_count_non_production
      memory        = 512
    }
  }

  service_configs_scaling_production = {
    authority = {
      cpu           = 256
      desired_count = local.desired_count_production
      memory        = 512
    }
    av_scanner_app = {
      cpu           = 256
      desired_count = local.desired_count_non_production
      memory        = 512
    }
    data_sharing = {
      cpu           = 256
      desired_count = local.desired_count_production
      memory        = 512
    }
    entity_verification = {
      cpu           = 256
      desired_count = local.desired_count_production
      memory        = 512
    }
    entity_verification_migrations = {
      cpu           = 256
      desired_count = local.desired_count_production
      memory        = 512
    }
    forms = {
      cpu           = 256
      desired_count = local.desired_count_production
      memory        = 512
    }
    organisation = {
      cpu           = 256
      desired_count = local.desired_count_production
      memory        = 512
    }
    organisation_app = {
      cpu           = 256
      desired_count = local.frontend_desired_count_non_production
      memory        = 512
    }
    organisation_information_migrations = {
      cpu           = 256
      desired_count = local.desired_count_production
      memory        = 512
    }
    person = {
      cpu           = 256
      desired_count = local.desired_count_production
      memory        = 512
    }
    tenant = {
      cpu           = 256
      desired_count = local.desired_count_production
      memory        = 512
    }
  }

  # @TODO (ABN) Remove me
  desired_count_development    = 1
  service_configs_scaling_development = {
    authority = {
      cpu           = 256
      desired_count = local.desired_count_development
      memory        = 512
    }
    av_scanner_app = {
      cpu           = 256
      desired_count = local.desired_count_non_production
      memory        = 512
    }
    data_sharing = {
      cpu           = 256
      desired_count = local.desired_count_development
      memory        = 512
    }
    entity_verification = {
      cpu           = 256
      desired_count = local.desired_count_development
      memory        = 512
    }
    entity_verification_migrations = {
      cpu           = 256
      desired_count = local.desired_count_development
      memory        = 512
    }
    forms = {
      cpu           = 256
      desired_count = local.desired_count_development
      memory        = 512
    }
    organisation = {
      cpu           = 256
      desired_count = local.desired_count_development
      memory        = 512
    }
    organisation_app = {
      cpu           = 256
      desired_count = local.frontend_desired_count_non_production
      memory        = 512
    }
    organisation_information_migrations = {
      cpu           = 256
      desired_count = local.desired_count_development
      memory        = 512
    }
    person = {
      cpu           = 256
      desired_count = local.desired_count_development
      memory        = 512
    }
    tenant = {
      cpu           = 256
      desired_count = local.desired_count_development
      memory        = 512
    }
  }

  service_configs_scaling = {
    development = local.service_configs_scaling_development
    staging     = local.service_configs_scaling_non_production
    integration = local.service_configs_scaling_non_production
  }

  service_configs_common = {
    authority = {
      name      = "authority"
      port      = 8092
      port_host = 8092
    }
    av_scanner_app = {
      name      = "av-scanner-app"
      port      = 8095
      port_host = 8095
    }
    data_sharing = {
      name      = "data-sharing"
      port      = 8088
      port_host = 8088
    }
    entity_verification = {
      name      = "entity-verification"
      port      = 8094
      port_host = 8094
    }
    entity_verification_migrations = {
      name      = "entity-verification-migrations"
      port      = 9191
      port_host = null
    }
    forms = {
      name      = "forms"
      port      = 8086
      port_host = 8086
    }
    organisation = {
      name      = "organisation"
      port      = 8082
      port_host = 8082
    }
    organisation_app = {
      name      = "organisation-app"
      port      = 8090
      port_host = 80
    }
    organisation_information_migrations = {
      name      = "organisation-information-migrations"
      port      = 9090
      port_host = null
    }
    person = {
      name      = "person"
      port      = 8084
      port_host = 8084
    }
    tenant = {
      name      = "tenant"
      port      = 8080
      port_host = 8080
    }
  }

  service_configs = {
    for key, value in try(local.service_configs_scaling[local.environment], local.service_configs_scaling_production) :
    key => merge(value, local.service_configs_common[key])
  }

  tags = {
    environment = local.environment
    managed_by  = "terragrunt"
  }

  tools_configs = {
    clamav_rest = {
      cpu       = 1024
      memory    = 3072
      name      = "clamav-rest"
      port      = 9000
      port_host = 9000
    }
    grafana = {
      cpu       = 256
      memory    = 512
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
