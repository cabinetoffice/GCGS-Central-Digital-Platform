terraform {
  source = local.global_vars.locals.environment == "orchestrator" ? "../../../modules//orchestrator/ecr" : null
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
      component = "orchestrator-ecr"
    }
  )

  account_ids = {
    for name, env in local.global_vars.locals.environments : name => env.account_id
  }
}

inputs = {
  account_ids     = local.account_ids
  service_configs = local.global_vars.locals.service_configs
  tags            = local.tags
}
