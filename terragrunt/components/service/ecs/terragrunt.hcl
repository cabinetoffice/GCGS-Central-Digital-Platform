terraform {
  source = local.global_vars.locals.environment != "orchestrator" ? "../../../modules//ecs" : null
}

include {
  path = find_in_parent_folders()
}

locals {
  global_vars = read_terragrunt_config(find_in_parent_folders("terragrunt.hcl"))
  service_vars = read_terragrunt_config(find_in_parent_folders("service.hcl"))

  tags = merge(
    local.global_vars.inputs.tags,
    local.service_vars.inputs.tags,
    {
      component = "ecs"
    }
  )

}

dependency core_iam {
  config_path = "../../core/iam"
  mock_outputs = {
    cloudwatch_events_arn               = "mock"
    cloudwatch_events_name              = "mock"
    ecs_task_arn                        = "mock"
    ecs_task_name                       = "mock"
    ecs_task_exec_arn                   = "mock"
    ecs_task_exec_name                  = "mock"
    service_deployer_step_function_arn  = "mock"
    service_deployer_step_function_name = "mock"
    terraform_arn                       = "mock"
    terraform_name                      = "mock"
  }
}

dependency core_networking {
  config_path = "../../core/networking"
  mock_outputs = {
    private_subnet_ids          = "mock"
    private_subnets_cidr_blocks = "mock"
    public_domain               = "mock"
    public_hosted_zone_id       = "mock"
    public_subnet_ids           = "mock"
    public_subnets_cidr_blocks  = "mock"
    vpc_id                      = "mock"
  }
}

dependency core_security_groups {
  config_path = "../../core/security-groups"
  mock_outputs = {
    alb_sg_id                 = "mock"
    db_postgres_sg_id         = "mock"
    ecs_sg_id                 = "mock"
    vpce_ecr_api_sg_id        = "mock"
    vpce_ecr_dkr_sg_id        = "mock"
    vpce_s3_sg_id             = "mock"
    vpce_secretsmanager_sg_id = "mock"
  }
}

dependency common_networking {
  config_path = "../../common/networking"
  mock_outputs = {
    vpce_s3_prefix_list_id = "mock"
  }
}

dependency service_auth {
  config_path = "../../service/auth"
  mock_outputs = {
    organisation_app_user_pool_arn       = "mock"
    organisation_app_user_pool_client_id = "mock"
    user_pool_domain                     = "mock"
  }
}

dependency service_database {
  config_path = "../../service/database"
  mock_outputs = {
    entity_verification_address         = "mock"
    entity_verification_credentials_arn = "mock"
    entity_verification_kms_arn         = "mock"
    entity_verification_name            = "mock"
    sirsi_address                       = "mock"
    sirsi_credentials_arn               = "mock"
    sirsi_kms_arn                       = "mock"
    sirsi_name                          = "mock"

  }
}

dependency service_queue {
  config_path = "../../service/queue"
  mock_outputs = {
    entity_verification_queue_arn = "mock"
    entity_verification_queue_url = "mock"
    organisation_queue_arn        = "mock"
    organisation_queue_url        = "mock"
  }
}

inputs = {

  account_ids            = local.global_vars.locals.account_ids
  pinned_service_version = local.global_vars.locals.pinned_service_version
  service_configs        = local.global_vars.locals.service_configs
  tags                   = local.tags

  role_cloudwatch_events_arn               = dependency.core_iam.outputs.cloudwatch_events_arn
  role_cloudwatch_events_name              = dependency.core_iam.outputs.cloudwatch_events_name
  role_ecs_task_arn                        = dependency.core_iam.outputs.ecs_task_arn
  role_ecs_task_name                       = dependency.core_iam.outputs.ecs_task_name
  role_ecs_task_exec_arn                   = dependency.core_iam.outputs.ecs_task_exec_arn
  role_ecs_task_exec_name                  = dependency.core_iam.outputs.ecs_task_exec_name
  role_service_deployer_step_function_arn  = dependency.core_iam.outputs.service_deployer_step_function_arn
  role_service_deployer_step_function_name = dependency.core_iam.outputs.service_deployer_step_function_name
  role_terraform_arn                       = dependency.core_iam.outputs.terraform_arn
  role_terraform_name                      = dependency.core_iam.outputs.terraform_name

  vpce_s3_prefix_list_id = dependency.common_networking.outputs.vpce_s3_prefix_list_id

  private_subnet_ids          = dependency.core_networking.outputs.private_subnet_ids
  private_subnets_cidr_blocks = dependency.core_networking.outputs.private_subnets_cidr_blocks
  public_domain               = dependency.core_networking.outputs.public_domain
  public_hosted_zone_id       = dependency.core_networking.outputs.public_hosted_zone_id
  public_subnet_ids           = dependency.core_networking.outputs.public_subnet_ids
  public_subnets_cidr_blocks  = dependency.core_networking.outputs.public_subnets_cidr_blocks
  vpc_id                      = dependency.core_networking.outputs.vpc_id
  vpc_cider                   = dependency.core_networking.outputs.vpc_cider


  alb_sg_id                 = dependency.core_security_groups.outputs.alb_sg_id
  db_postgres_sg_id         = dependency.core_security_groups.outputs.db_postgres_sg_id
  ecs_sg_id                 = dependency.core_security_groups.outputs.ecs_sg_id
  vpce_ecr_api_sg_id        = dependency.core_security_groups.outputs.vpce_ecr_api_sg_id
  vpce_ecr_dkr_sg_id        = dependency.core_security_groups.outputs.vpce_ecr_dkr_sg_id
  vpce_logs_sg_id           = dependency.core_security_groups.outputs.vpce_logs_sg_id
  vpce_s3_sg_id             = dependency.core_security_groups.outputs.vpce_s3_sg_id
  vpce_secretsmanager_sg_id = dependency.core_security_groups.outputs.vpce_secretsmanager_sg_id

  user_pool_arn       = dependency.service_auth.outputs.organisation_app_user_pool_arn
  user_pool_client_id = dependency.service_auth.outputs.organisation_app_user_pool_client_id
  user_pool_domain    = dependency.service_auth.outputs.user_pool_domain

  db_entity_verification_address         = dependency.service_database.outputs.entity_verification_address
  db_entity_verification_credentials_arn = dependency.service_database.outputs.entity_verification_credentials_arn
  db_entity_verification_kms_arn         = dependency.service_database.outputs.entity_verification_kms_arn
  db_entity_verification_name            = dependency.service_database.outputs.entity_verification_name
  db_sirsi_address                       = dependency.service_database.outputs.sirsi_address
  db_sirsi_credentials_arn               = dependency.service_database.outputs.sirsi_credentials_arn
  db_sirsi_kms_arn                       = dependency.service_database.outputs.sirsi_kms_arn
  db_sirsi_name                          = dependency.service_database.outputs.sirsi_name


  queue_entity_verification_queue_arn = dependency.service_queue.outputs.entity_verification_queue_arn
  queue_entity_verification_queue_url = dependency.service_queue.outputs.entity_verification_queue_url
  queue_organisation_queue_arn        = dependency.service_queue.outputs.organisation_queue_arn
  queue_organisation_queue_url        = dependency.service_queue.outputs.organisation_queue_url
}
