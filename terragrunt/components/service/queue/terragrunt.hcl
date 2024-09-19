terraform {
  source = local.global_vars.locals.environment != "orchestrator" ? "../../../modules//queue" : null
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
      component = "queue"
    }
  )
}

dependency core_iam {
  config_path = "../../core/iam"
  mock_outputs = {
    ecs_task_arn = "mock"
  }
}

inputs = {
  tags = local.tags

  role_ecs_task_arn = dependency.core_iam.outputs.ecs_task_arn
}
