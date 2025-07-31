terraform {
  source = local.global_vars.locals.environment != "orchestrator" ? "../../../modules//notification" : null
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
      component = "notification"
    }
  )
}

dependency core_networking {
  config_path = "../../core/networking"
  mock_outputs = {
    public_hosted_zone_id        = "mock"
  }
}

dependency core_iam {
  config_path = "../../core/iam"
  mock_outputs = {
    ecs_task_arn = "mock"
  }
}

inputs = {
  mail_from_domains = local.global_vars.locals.mail_from_domains
  tags              = local.tags


  role_ecs_task_arn = dependency.core_iam.outputs.ecs_task_arn

  public_hosted_zone_id = dependency.core_networking.outputs.public_hosted_zone_id
}
