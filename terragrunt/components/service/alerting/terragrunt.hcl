terraform {
  source = local.global_vars.locals.environment != "orchestrator" ? "../../../modules//alerting" : null
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
      component = "alerting"
    }
  )

}

dependency service_cache {
  config_path = "../../service/cache"
  mock_outputs = {
    redis_cluster_node_ids = "mock"
  }
}

inputs = {
  service_configs = local.global_vars.locals.service_configs
  tags            = local.tags

  redis_cluster_node_ids = dependency.service_cache.outputs.redis_cluster_node_ids
}
