terraform {
  source = "../../../modules//api-gateway"
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
      component = "api-gateway"
    }
  )

}

dependency core_iam {
  config_path = "../../core/iam"
  mock_outputs = {
    api_gateway_cloudwatch_arn  = "mock"
  }
}

dependency service_ecs {
  config_path = "../../service/ecs"
  mock_outputs = {
    lb_ecs_dns_name = "mock"
  }
}

inputs = {

  service_configs = local.global_vars.locals.service_configs
  tags            = local.tags

  role_api_gateway_cloudwatch_arn  = dependency.core_iam.outputs.api_gateway_cloudwatch_arn

  lb_ecs_dns_name = dependency.service_ecs.outputs.lb_ecs_dns_name
}
