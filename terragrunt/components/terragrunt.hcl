locals {

    cidr_b_development = 3
    cidr_b_production = 1
    cidr_b_staging = 2

    environment = get_env("TG_ENVIRONMENT", "development")

    environments = {
        development = {
            cidr_block             = "10.${local.cidr_b_development}.0.0/16"
            name                   = "development"
            postgres_instance_type = "db.t4g.micro"
            private_subnets        = [
                "10.${local.cidr_b_development}.101.0/24",
                "10.${local.cidr_b_development}.102.0/24",
                "10.${local.cidr_b_development}.103.0/24"
            ]
            public_subnets = [
                "10.${local.cidr_b_development}.1.0/24",
                "10.${local.cidr_b_development}.2.0/24",
                "10.${local.cidr_b_development}.3.0/24"
            ]
        }
        staging = {
            cidr_block             = "10.${local.cidr_b_staging}.0.0/16"
            name                   = "staging"
            postgres_instance_type = "db.t4g.micro"
            private_subnets        = [
                "10.${local.cidr_b_staging}.101.0/24",
                "10.${local.cidr_b_staging}.102.0/24",
                "10.${local.cidr_b_staging}.103.0/24"
            ]
            public_subnets = [
                "10.${local.cidr_b_staging}.1.0/24",
                "10.${local.cidr_b_staging}.2.0/24",
                "10.${local.cidr_b_staging}.3.0/24"
            ]
        }
    }

    product = {
        name               = "CDP SIRSI"
        resource_name      = "cdp-sirsi"
        public_hosted_zone = "cdp-sirsi.civilservice.gov.uk"
    }


    tg = {
        state_bucket = "tfstate-${local.product.resource_name}-${local.environment}-${get_aws_account_id()}"
        state_key    = "${path_relative_to_include()}/terraform.tfstate"
    }

    tags = {
        environment    = local.environment
        managed_by     = "terragrunt"
    }

    versions = {
        postgres_engine = "16.2"
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
    environment             = local.environment
    product                 = local.product
    tags                    = local.tags
    tfstate_bucket_name     = local.tg.state_bucket
    postgres_engine_version = local.versions.postgres_engine
    postgres_instance_type  = local.environments[local.environment].postgres_instance_type
    vpc_cidr                = local.environments[local.environment].cidr_block
    vpc_private_subnets     = local.environments[local.environment].private_subnets
    vpc_public_subnets      = local.environments[local.environment].public_subnets
}

terraform {
    extra_arguments disable_input {
        commands  = get_terraform_commands_that_need_input()
        arguments = ["-input=false"]
    }
}
