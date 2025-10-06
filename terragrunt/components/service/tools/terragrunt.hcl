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
    cloudwatch_events_arn               = "mock"
    cloudwatch_events_name              = "mock"
    ecs_task_arn                        = "mock"
    ecs_task_exec_arn                   = "mock"
    ecs_task_exec_name                  = "mock"
    ecs_task_name                       = "mock"
    rds_cloudwatch_arn                  = "mock"
    service_deployer_step_function_arn  = "mock"
    service_deployer_step_function_name = "mock"
    terraform_arn                       = "mock"
  }
}

dependency core_networking {
  config_path = "../../core/networking"
  mock_outputs = {
    private_subnet_ids    = "mock"
    public_domain         = "mock"
    public_hosted_zone_id = "mock"
    public_subnet_ids     = "mock"
    vpc_id                = "mock"
    waf_acl_tools_arn     = "mock"
  }
}

dependency core_security_groups {
  config_path = "../../core/security-groups"
  mock_outputs = {
    alb_tools_sg_id   = "mock"
    db_mysql_sg_id    = "mock"
    db_postgres_sg_id = "mock"
    ecs_sg_id         = "mock"
    efs_sg_id         = "mock"
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

dependency service_cache {
  config_path = "../../service/cache"
  mock_outputs = {
    port                     = "mock"
    primary_endpoint_address = "mock"
    redis_auth_token_arn     = "mock"
  }
}

dependency service_ecs {
  config_path = "../../service/ecs"
  mock_outputs = {
    certificate_arn  = "mock"
    ecs_alb_dns_name = "mock"
    ecs_cluster_id   = "mock"
    ecs_cluster_name = "mock"
  }
}

dependency service_database {
  config_path = "../../service/database"
  mock_outputs = {
    cfs_cluster_address                         = "mock"
    cfs_cluster_credentials_arn                 = "mock"
    cfs_cluster_name                            = "mock"
    entity_verification_cluster_address         = "mock"
    entity_verification_cluster_credentials_arn = "mock"
    entity_verification_cluster_name            = "mock"
    fts_cluster_address                         = "mock"
    fts_cluster_credentials_arn                 = "mock"
    fts_cluster_name                            = "mock"
    sirsi_cluster_address                       = "mock"
    sirsi_cluster_credentials_arn               = "mock"
    sirsi_cluster_name                          = "mock"
    sirsi_credentials_arn                       = "mock"
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
  account_ids         = local.global_vars.locals.account_ids
  cloud_beaver_config = local.global_vars.locals.tools_configs.cloud_beaver
  healthcheck_config  = local.global_vars.locals.tools_configs.healthcheck
  tools_configs       = local.global_vars.locals.tools_configs
  tags                = local.tags

  role_cloudwatch_events_arn               = dependency.core_iam.outputs.cloudwatch_events_arn
  role_cloudwatch_events_name              = dependency.core_iam.outputs.cloudwatch_events_name
  role_ecs_task_arn                        = dependency.core_iam.outputs.ecs_task_arn
  role_ecs_task_exec_arn                   = dependency.core_iam.outputs.ecs_task_exec_arn
  role_ecs_task_exec_name                  = dependency.core_iam.outputs.ecs_task_exec_name
  role_ecs_task_name                       = dependency.core_iam.outputs.ecs_task_name
  role_rds_cloudwatch_arn                  = dependency.core_iam.outputs.rds_cloudwatch_arn
  role_service_deployer_step_function_arn  = dependency.core_iam.outputs.service_deployer_step_function_arn
  role_service_deployer_step_function_name = dependency.core_iam.outputs.service_deployer_step_function_name
  role_terraform_arn                       = dependency.core_iam.outputs.terraform_arn

  private_subnet_ids    = dependency.core_networking.outputs.private_subnet_ids
  public_domain         = dependency.core_networking.outputs.public_domain
  public_hosted_zone_id = dependency.core_networking.outputs.public_hosted_zone_id
  public_subnet_ids     = dependency.core_networking.outputs.public_subnet_ids
  vpc_id                = dependency.core_networking.outputs.vpc_id
  waf_acl_tools_arn     = dependency.core_networking.outputs.waf_acl_tools_arn

  alb_tools_sg_id   = dependency.core_security_groups.outputs.alb_tools_sg_id
  db_mysql_sg_id    = dependency.core_security_groups.outputs.db_mysql_sg_id
  db_postgres_sg_id = dependency.core_security_groups.outputs.db_postgres_sg_id
  ecs_alb_sg_id     = dependency.core_security_groups.outputs.alb_sg_id
  ecs_sg_id         = dependency.core_security_groups.outputs.ecs_sg_id
  efs_sg_id         = dependency.core_security_groups.outputs.efs_sg_id

  user_pool_arn_cloud_beaver       = dependency.service_auth.outputs.cloud_beaver_user_pool_arn
  user_pool_arn_healthcheck        = dependency.service_auth.outputs.healthcheck_user_pool_arn
  user_pool_client_id_cloud_beaver = dependency.service_auth.outputs.cloud_beaver_user_pool_client_id
  user_pool_client_id_healthcheck  = dependency.service_auth.outputs.healthcheck_user_pool_client_id
  user_pool_domain_cloud_beaver    = dependency.service_auth.outputs.user_pool_domain
  user_pool_domain_healthcheck     = dependency.service_auth.outputs.user_pool_domain

  certificate_arn  = dependency.service_ecs.outputs.certificate_arn
  ecs_cluster_id   = dependency.service_ecs.outputs.ecs_cluster_id
  ecs_cluster_name = dependency.service_ecs.outputs.ecs_cluster_name
  ecs_alb_dns_name = dependency.service_ecs.outputs.ecs_alb_dns_name

  db_cfs_cluster_address                 = dependency.service_database.outputs.cfs_cluster_address
  db_cfs_cluster_credentials_arn         = dependency.service_database.outputs.cfs_cluster_credentials_arn
  db_cfs_cluster_name                    = dependency.service_database.outputs.cfs_cluster_name
  db_ev_cluster_address                  = dependency.service_database.outputs.entity_verification_cluster_address
  db_ev_cluster_credentials_arn          = dependency.service_database.outputs.entity_verification_cluster_credentials_arn
  db_ev_cluster_name                     = dependency.service_database.outputs.entity_verification_cluster_name
  db_fts_cluster_address                 = dependency.service_database.outputs.fts_cluster_address
  db_fts_cluster_credentials_arn         = dependency.service_database.outputs.fts_cluster_credentials_arn
  db_fts_cluster_name                    = dependency.service_database.outputs.fts_cluster_name
  db_sirsi_cluster_address               = dependency.service_database.outputs.sirsi_cluster_address
  db_sirsi_cluster_credentials_arn       = dependency.service_database.outputs.sirsi_cluster_credentials_arn
  db_sirsi_cluster_name                  = dependency.service_database.outputs.sirsi_cluster_name

  redis_primary_endpoint = dependency.service_cache.outputs.primary_endpoint_address
  redis_auth_token_arn   = dependency.service_cache.outputs.redis_auth_token_arn
  redis_port             = dependency.service_cache.outputs.port

  sqs_entity_verification_url = dependency.service_queue.outputs.entity_verification_queue_url
  sqs_organisation_url        = dependency.service_queue.outputs.organisation_queue_url
}
