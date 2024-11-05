terraform {
  source = local.global_vars.locals.environment == "integration" ? "../../../modules//externals/networking" : null
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
      component = "networking"
    }
  )

}

inputs = {
  tags                = local.tags
}
