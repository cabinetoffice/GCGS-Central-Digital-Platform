terraform {
  source = local.global_vars.locals.environment != "orchestrator" ? "../../../modules//core-networking" : null
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
      component = "networking"
    }
  )
}

inputs = {
  tags = local.tags
}
