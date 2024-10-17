terraform {
  source = local.global_vars.locals.environment != "orchestrator" ? "../../../modules//auth" : null
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
      component = "auth"
    }
  )

}

dependency core_networking {
  config_path = "../../core/networking"
  mock_outputs = {
    public_domain = "mock"
  }
}


inputs = {
  tags = local.tags

  public_domain = dependency.core_networking.outputs.public_domain
}
