terraform {
  source = local.global_vars.locals.environment == "orchestrator" ? null : "../../../modules//tools"
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
      component = "tools"
    }
  )

}

dependency core_iam {
  config_path = "../../core/iam"
  mock_outputs = {
    ecs_task_arn       = "mock"
    ecs_task_exec_arn  = "mock"
    ecs_task_exec_name = "mock"
    ecs_task_name      = "mock"
    rds_cloudwatch_arn = "mock"
    terraform_arn      = "mock"
  }
}

dependency core_networking {
  config_path = "../../core/networking"
  mock_outputs = {
    private_subnet_ids    = "mock"
    public_domain         = "mock"
    public_hosted_zone_id = "mock"
    vpc_id                = "mock"
  }
}

dependency core_security_groups {
  config_path = "../../core/security-groups"
  mock_outputs = {
    alb_sg_id         = "mock"
    db_postgres_sg_id = "mock"
    ecs_sg_id         = "mock"
  }
}

dependency service_auth {
  config_path = "../../service/auth"
  mock_outputs = {
    healthcheck_user_pool_arn       = "mock"
    healthcheck_user_pool_client_id = "mock"
    user_pool_domain                = "mock"
  }
}

dependency service_ecs {
  config_path = "../../service/ecs"
  mock_outputs = {
    ecs_cluster_id  = "mock"
    ecs_cluster_id  = "mock"
    ecs_lb_dns_name = "mock"
  }
}

dependency service_database {
  config_path = "../../service/database"
  mock_outputs = {
    entity_verification_address               = "mock"
    entity_verification_connection_secret_arn = "mock"
    entity_verification_credentials_arn       = "mock"
    entity_verification_kms_arn               = "mock"
    entity_verification_name                  = "mock"
    sirsi_address                             = "mock"
    sirsi_connection_secret_arn               = "mock"
    sirsi_credentials_arn                     = "mock"
    sirsi_kms_arn                             = "mock"
    sirsi_name                                = "mock"
  }
}

dependency service_queue {
  config_path = "../../service/queue"
  mock_outputs = {
    healthcheck_queue_arn = "mock"
    healthcheck_queue_url = "mock"
  }
}

inputs = {
  account_ids        = local.global_vars.locals.account_ids
  healthcheck_config = local.global_vars.locals.tools_configs.healthcheck
  pgadmin_config     = local.global_vars.locals.tools_configs.pgadmin
  tags               = local.tags

  role_ecs_task_arn       = dependency.core_iam.outputs.ecs_task_arn
  role_ecs_task_name      = dependency.core_iam.outputs.ecs_task_name
  role_ecs_task_exec_arn  = dependency.core_iam.outputs.ecs_task_exec_arn
  role_ecs_task_exec_name = dependency.core_iam.outputs.ecs_task_exec_name
  role_rds_cloudwatch_arn = dependency.core_iam.outputs.rds_cloudwatch_arn
  role_terraform_arn      = dependency.core_iam.outputs.terraform_arn

  private_subnet_ids    = dependency.core_networking.outputs.private_subnet_ids
  public_domain         = dependency.core_networking.outputs.public_domain
  public_hosted_zone_id = dependency.core_networking.outputs.public_hosted_zone_id
  vpc_id                = dependency.core_networking.outputs.vpc_id

  db_postgres_sg_id = dependency.core_security_groups.outputs.db_postgres_sg_id
  ecs_alb_sg_id     = dependency.core_security_groups.outputs.alb_sg_id
  ecs_sg_id         = dependency.core_security_groups.outputs.ecs_sg_id

  user_pool_arn       = dependency.service_auth.outputs.healthcheck_user_pool_arn
  user_pool_client_id = dependency.service_auth.outputs.healthcheck_user_pool_client_id
  user_pool_domain    = dependency.service_auth.outputs.user_pool_domain

  ecs_cluster_id   = dependency.service_ecs.outputs.ecs_cluster_id
  ecs_lb_dns_name  = dependency.service_ecs.outputs.ecs_lb_dns_name
  ecs_listener_arn = dependency.service_ecs.outputs.ecs_listener_arn

  db_entity_verification_address         = dependency.service_database.outputs.entity_verification_address
  db_entity_verification_credentials_arn = dependency.service_database.outputs.entity_verification_credentials_arn
  db_entity_verification_kms_arn         = dependency.service_database.outputs.entity_verification_kms_arn
  db_entity_verification_name            = dependency.service_database.outputs.entity_verification_name
  db_sirsi_address                       = dependency.service_database.outputs.sirsi_address
  db_sirsi_credentials_arn               = dependency.service_database.outputs.sirsi_credentials_arn
  db_sirsi_kms_arn                       = dependency.service_database.outputs.sirsi_kms_arn
  db_sirsi_name                          = dependency.service_database.outputs.sirsi_name

  queue_healthcheck_queue_arn = dependency.service_queue.outputs.healthcheck_queue_arn
  queue_healthcheck_queue_url = dependency.service_queue.outputs.healthcheck_queue_url
}
