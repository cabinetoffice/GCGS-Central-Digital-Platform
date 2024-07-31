terraform {
  source = contains(["development", "staging", "integration"], local.global_vars.locals.environment) ? "../../../modules//tools" : null
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
    ecs_task_arn      = "mock"
    ecs_task_name     = "mock"
    ecs_task_exec_arn = "mock"
  }
}

dependency core_networking {
  config_path = "../../core/networking"
  mock_outputs = {
    private_subnet_ids      = "mock"
    public_hosted_zone_fqdn = "mock"
    public_hosted_zone_id   = "mock"
    vpc_id                  = "mock"
  }
}

dependency core_security_groups {
  config_path = "../../core/security-groups"
  mock_outputs = {
    alb_sg_id = "mock"
    ecs_sg_id = "mock"
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
    entity_verification_credentials           = "mock"
    entity_verification_kms_arn               = "mock"
    entity_verification_name                  = "mock"
    sirsi_address                             = "mock"
    sirsi_connection_secret_arn               = "mock"
    sirsi_credentials                         = "mock"
    sirsi_kms_arn                             = "mock"
    sirsi_name                                = "mock"
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
  account_ids        = local.global_vars.locals.account_ids
  healthcheck_config = local.global_vars.locals.tools_configs.healthcheck
  pgadmin_config     = local.global_vars.locals.tools_configs.pgadmin
  tags               = local.tags

  role_ecs_task_arn      = dependency.core_iam.outputs.ecs_task_arn
  role_ecs_task_name     = dependency.core_iam.outputs.ecs_task_name
  role_ecs_task_exec_arn = dependency.core_iam.outputs.ecs_task_exec_arn

  private_subnet_ids      = dependency.core_networking.outputs.private_subnet_ids
  public_hosted_zone_fqdn = dependency.core_networking.outputs.public_hosted_zone_fqdn
  public_hosted_zone_id   = dependency.core_networking.outputs.public_hosted_zone_id
  vpc_id                  = dependency.core_networking.outputs.vpc_id

  ecs_alb_sg_id = dependency.core_security_groups.outputs.alb_sg_id
  ecs_sg_id     = dependency.core_security_groups.outputs.ecs_sg_id

  ecs_cluster_id   = dependency.service_ecs.outputs.ecs_cluster_id
  ecs_lb_dns_name  = dependency.service_ecs.outputs.ecs_lb_dns_name
  ecs_listener_arn = dependency.service_ecs.outputs.ecs_listener_arn

  db_entity_verification_address               = dependency.service_database.outputs.entity_verification_address
  db_entity_verification_credentials           = dependency.service_database.outputs.entity_verification_credentials
  db_entity_verification_kms_arn               = dependency.service_database.outputs.entity_verification_kms_arn
  db_entity_verification_name                  = dependency.service_database.outputs.entity_verification_name
  db_sirsi_address                             = dependency.service_database.outputs.sirsi_address
  db_sirsi_credentials                         = dependency.service_database.outputs.sirsi_credentials
  db_sirsi_kms_arn                             = dependency.service_database.outputs.sirsi_kms_arn
  db_sirsi_name                                = dependency.service_database.outputs.sirsi_name

  queue_entity_verification_queue_arn = dependency.service_queue.outputs.entity_verification_queue_arn
  queue_entity_verification_queue_url = dependency.service_queue.outputs.entity_verification_queue_url
  queue_organisation_queue_arn        = dependency.service_queue.outputs.organisation_queue_arn
  queue_organisation_queue_url        = dependency.service_queue.outputs.organisation_queue_url
}
