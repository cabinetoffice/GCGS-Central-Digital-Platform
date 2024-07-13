terraform {
  source = local.global_vars.locals.environment == "orchestrator" ? "../../../modules//orchestrator/iam" : null
}

include {
  path = find_in_parent_folders()
}

locals {

  global_vars = read_terragrunt_config(find_in_parent_folders("terragrunt.hcl"))
  core_vars   = read_terragrunt_config(find_in_parent_folders("orchestrator.hcl"))

  tags = merge(
    local.global_vars.inputs.tags,
    local.core_vars.inputs.tags,
    {
      component = "orchestrator-iam"
    }
  )
}

inputs = {
  tags                = local.tags
  terraform_operators = local.global_vars.locals.terraform_operators
}
