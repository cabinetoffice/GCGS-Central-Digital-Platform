terraform {
  source = local.global_vars.locals.environment != "orchestrator" ? "../../../modules//api-gateway" : null
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
    api_gateway_cloudwatch_arn              = "mock"
    api_gateway_deployer_step_function_arn  = "mock"
    api_gateway_deployer_step_function_name = "mock"
  }
}

dependency core_networking {
  config_path = "../../core/networking"
  mock_outputs = {
    public_hosted_zone_fqdn = "mock"
    public_hosted_zone_id   = "mock"
  }
}

dependency service_ecs {
  config_path = "../../service/ecs"
  mock_outputs = {
    certificate_arn = "mock"
  }
}

inputs = {
  account_ids     = local.global_vars.locals.account_ids
  service_configs = local.global_vars.locals.service_configs
  tags            = local.tags

  role_api_gateway_cloudwatch_arn = dependency.core_iam.outputs.api_gateway_cloudwatch_arn
  role_api_gateway_deployer_step_function_arn = dependency.core_iam.outputs.api_gateway_deployer_step_function_arn
  role_api_gateway_deployer_step_function_name = dependency.core_iam.outputs.api_gateway_deployer_step_function_name

  public_hosted_zone_fqdn = dependency.core_networking.outputs.public_hosted_zone_fqdn
  public_hosted_zone_id   = dependency.core_networking.outputs.public_hosted_zone_id

  certificate_arn = dependency.service_ecs.outputs.certificate_arn
}
