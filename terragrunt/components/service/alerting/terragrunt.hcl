terraform {
  source = local.global_vars.locals.environment != "orchestrator" ? "../../../modules//alerting" : null
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

dependency service_database {
  config_path = "../../service/database"
  mock_outputs = {
    cluster_ids = "mock"
  }
}

dependency service_ecs {
  config_path = "../../service/ecs"
  mock_outputs = {
    ecs_alb_arn_suffix                       = "mock"
    ecs_cluster_name                         = "mock"
    services_target_group_arn_suffix_map     = "mock"
    fts_ecs_alb_arn_suffix                   = "mock"
    services_target_group_arn_suffix_map_fts = "mock"
  }
}

dependency service_queue {
  config_path = "../../service/queue"
  mock_outputs = {
    queue_names = "mock"
  }
}

inputs = {
  service_configs = local.global_vars.locals.service_configs
  tags            = local.tags

  redis_cluster_node_ids = dependency.service_cache.outputs.redis_cluster_node_ids

  rds_cluster_ids = dependency.service_database.outputs.cluster_ids

  ecs_alb_arn_suffix                           = dependency.service_ecs.outputs.ecs_alb_arn_suffix
  ecs_cluster_name                             = dependency.service_ecs.outputs.ecs_cluster_name
  ecs_services_target_group_arn_suffix_map     = dependency.service_ecs.outputs.services_target_group_arn_suffix_map
  ecs_fts_alb_arn_suffix                       = dependency.service_ecs.outputs.fts_ecs_alb_arn_suffix
  ecs_fts_services_target_group_arn_suffix_map = dependency.service_ecs.outputs.services_target_group_arn_suffix_map_fts

  sqs_queue_names = dependency.service_queue.outputs.queue_names
}
