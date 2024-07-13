terraform {
  source = local.global_vars.locals.environment != "orchestrator" ? "../../../modules//core-security-groups" : null
}

include {
  path = find_in_parent_folders()
}

locals {
  global_vars = read_terragrunt_config(find_in_parent_folders("terragrunt.hcl"))
  core_vars   = read_terragrunt_config(find_in_parent_folders("core.hcl"))

  tags = merge(
    local.global_vars.inputs.tags,
    local.core_vars.inputs.tags,
    {
      component = "security-groups"
    }
  )
}

dependency core_networking {
  config_path = "../networking"
  mock_outputs = {
    vpc_id = "mock"
  }
}

inputs = {
  tags = local.tags

  vpc_id = dependency.core_networking.outputs.vpc_id
}
