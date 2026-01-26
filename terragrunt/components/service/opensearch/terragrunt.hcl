terraform {
  source = local.global_vars.locals.environment == "development" ? "../../../modules//opensearch" : null
}

include {
  path = find_in_parent_folders("root.hcl")
}

locals {
  global_vars  = read_terragrunt_config(find_in_parent_folders("root.hcl"))
  service_vars = read_terragrunt_config(find_in_parent_folders("service.hcl"))

  tags = merge(
    local.global_vars.inputs.tags,
    local.service_vars.inputs.tags,
    {
      component = "opensearch"
    }
  )

}

dependency core_iam {
  config_path = "../../core/iam"
  mock_outputs = {
    opensearch_admin_arn           = "mock"
    ecs_task_arn                   = "mock"
    ecs_task_name                  = "mock"
    ecs_task_opensearch_admin_arn  = "mock"
    ecs_task_opensearch_admin_name = "mock"
    opensearch_admin_arn           = "mock"
  }
}

dependency core_networking {
  config_path = "../../core/networking"
  mock_outputs = {
    private_subnets = []
    private_subnets_cidr_blocks = []
    vpc_id = "mock"
  }
}

dependency core_security_groups {
  config_path = "../../core/security-groups"
  mock_outputs = {
    ecs_sg_id = "mock"
  }
}

inputs = {
  opensearch_sg_id = dependency.core_security_groups.outputs.opensearch_sg_id
  tags             = local.tags

  role_ecs_task_arn                   = dependency.core_iam.outputs.ecs_task_arn
  role_ecs_task_name                  = dependency.core_iam.outputs.ecs_task_name
  role_ecs_task_opensearch_admin_arn  = dependency.core_iam.outputs.ecs_task_opensearch_admin_arn
  role_ecs_task_opensearch_admin_name = dependency.core_iam.outputs.ecs_task_opensearch_admin_name
  role_opensearch_admin_arn           = dependency.core_iam.outputs.opensearch_admin_arn

  private_subnet_ids          = dependency.core_networking.outputs.private_subnet_ids
  private_subnets_cidr_blocks = dependency.core_networking.outputs.private_subnets_cidr_blocks

  ecs_sg_id = dependency.core_security_groups.outputs.ecs_sg_id
}
