terraform {
  source = local.global_vars.locals.environment == "integration" ? "../../../modules//externals/security-groups" : null
}

include {
  path = find_in_parent_folders()
}

locals {
  global_vars = read_terragrunt_config(find_in_parent_folders("terragrunt.hcl"))
  externals_vars = read_terragrunt_config(find_in_parent_folders("externals.hcl"))

  tags = merge(
    local.global_vars.inputs.tags,
    local.externals_vars.inputs.tags,
    {
      component = "security-groups"
    }
  )
}

dependency externals_networking {
  config_path = "../networking"
  mock_outputs = {
    vpc_id = "mock"
  }
}

inputs = {
  tags = local.tags

  vpc_id = dependency.externals_networking.outputs.vpc_id
}
