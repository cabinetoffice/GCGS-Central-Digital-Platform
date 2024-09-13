terraform {
  source = "../../../modules//core-iam"
}

include {
  path = find_in_parent_folders()
}

locals {
  global_vars = read_terragrunt_config(find_in_parent_folders("terragrunt.hcl"))
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
  pen_testing_allowed_ips        = local.global_vars.locals.pen_testing.allowed_ips
  pen_testing_user_arns          = local.global_vars.locals.pen_testing.user_arns
  pen_testing_external_user_arns = local.global_vars.locals.pen_testing.external_user_arns
  tags                           = local.tags
  terraform_operators            = local.global_vars.locals.terraform_operators
  tfstate_bucket_name            = local.global_vars.locals.tg.state_bucket
}
