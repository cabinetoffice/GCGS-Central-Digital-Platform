terraform {
  source = local.global_vars.locals.environment != "orchestrator" ? "../../../modules//elasticache" : null
}

include {
  path = find_in_parent_folders("root.hcl")
}

locals {
  global_vars = read_terragrunt_config(find_in_parent_folders("root.hcl"))
  service_vars = read_terragrunt_config(find_in_parent_folders("service.hcl"))

  tags = merge(
    local.global_vars.inputs.tags,
    local.service_vars.inputs.tags,
    {
      component = "cache"
    }
  )

}

dependency core_iam {
  config_path = "../../core/iam"
  mock_outputs = {
    terraform_arn  = "mock"
    terraform_name = "mock"
  }
}

dependency core_networking {
  config_path = "../../core/networking"
  mock_outputs = {
    private_subnets             = "mock"
    private_subnets_cidr_blocks = "mock"
  }
}

dependency core_security_groups {
  config_path = "../../core/security-groups"
  mock_outputs = {
    elasticache_redis_sg_id = "mock"
  }
}

inputs = {
  node_type = local.global_vars.locals.redis_node_type
  tags      = local.tags

  private_subnet_ids          = dependency.core_networking.outputs.private_subnet_ids
  private_subnets_cidr_blocks = dependency.core_networking.outputs.private_subnets_cidr_blocks

  elasticache_redis_sg_id = dependency.core_security_groups.outputs.elasticache_redis_sg_id
}
