terraform {
  source = "../../../modules//core-iam"
}

include {
  path = find_in_parent_folders("root.hcl")
}

locals {
  global_vars = read_terragrunt_config(find_in_parent_folders("root.hcl"))
  core_vars = read_terragrunt_config(find_in_parent_folders("core.hcl"))

  tags = merge(
    local.global_vars.inputs.tags,
    local.core_vars.inputs.tags,
    {
      component = "iam"
    }
  )
}

inputs = {
  account_ids                    = local.global_vars.locals.account_ids
  tags                           = local.tags
  tfstate_bucket_name            = local.global_vars.locals.tg.state_bucket
}
