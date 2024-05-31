terraform {
  source = "../../../modules//ecs"
}

include {
  path = find_in_parent_folders()
}

locals {
  global_vars  = read_terragrunt_config(find_in_parent_folders("terragrunt.hcl"))
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
    ecs_task_exec_arn                   = "mock"
    ecs_task_exec_name                  = "mock"
    service_deployer_step_function_arn  = "mock"
    service_deployer_step_function_name = "mock"

  }
}

dependency core_networking {
  config_path = "../../core/networking"
  mock_outputs = {
    private_subnet_ids          = "mock"
    private_subnets_cidr_blocks = "mock"
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
    ecs_service_base_sg_id    = "mock"
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

dependency service_database {
  config_path = "../../service/database"
  mock_outputs = {
    db_connection_secret_arn = "mock"
    db_kms_arn               = "mock"
  }
}

inputs = {
  tags = local.tags

  role_cloudwatch_events_arn               = dependency.core_iam.outputs.cloudwatch_events_arn
  role_cloudwatch_events_name              = dependency.core_iam.outputs.cloudwatch_events_name
  role_ecs_task_arn                        = dependency.core_iam.outputs.ecs_task_arn
  role_ecs_task_exec_arn                   = dependency.core_iam.outputs.ecs_task_exec_arn
  role_ecs_task_exec_name                  = dependency.core_iam.outputs.ecs_task_exec_name
  role_service_deployer_step_function_arn  = dependency.core_iam.outputs.service_deployer_step_function_arn
  role_service_deployer_step_function_name = dependency.core_iam.outputs.service_deployer_step_function_name

  vpce_s3_prefix_list_id = dependency.common_networking.outputs.vpce_s3_prefix_list_id

  private_subnet_ids          = dependency.core_networking.outputs.private_subnet_ids
  private_subnets_cidr_blocks = dependency.core_networking.outputs.private_subnets_cidr_blocks
  public_hosted_zone_id       = dependency.core_networking.outputs.public_hosted_zone_id
  public_subnet_ids           = dependency.core_networking.outputs.public_subnet_ids
  public_subnets_cidr_blocks  = dependency.core_networking.outputs.public_subnets_cidr_blocks
  vpc_id                      = dependency.core_networking.outputs.vpc_id


  alb_sg_id                 = dependency.core_security_groups.outputs.alb_sg_id
  db_postgres_sg_id         = dependency.core_security_groups.outputs.db_postgres_sg_id
  ecs_sg_id                 = dependency.core_security_groups.outputs.ecs_sg_id
  vpce_ecr_api_sg_id        = dependency.core_security_groups.outputs.vpce_ecr_api_sg_id
  vpce_ecr_dkr_sg_id        = dependency.core_security_groups.outputs.vpce_ecr_dkr_sg_id
  vpce_logs_sg_id           = dependency.core_security_groups.outputs.vpce_logs_sg_id
  vpce_s3_sg_id             = dependency.core_security_groups.outputs.vpce_s3_sg_id
  vpce_secretsmanager_sg_id = dependency.core_security_groups.outputs.vpce_secretsmanager_sg_id

  db_connection_secret_arn = dependency.service_database.outputs.db_connection_secret_arn
  db_kms_arn               = dependency.service_database.outputs.db_kms_arn

}
