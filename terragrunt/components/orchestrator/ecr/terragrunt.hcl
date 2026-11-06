terraform {
  source = local.global_vars.locals.environment == "orchestrator" ? "../../../modules//orchestrator/ecr" : null
}

include {
  path = find_in_parent_folders("root.hcl")
}

locals {

  global_vars       = read_terragrunt_config(find_in_parent_folders("root.hcl"))
  orchestrator_vars = read_terragrunt_config(find_in_parent_folders("orchestrator.hcl"))

  tags = merge(
    local.global_vars.inputs.tags,
    local.orchestrator_vars.inputs.tags,
    {
      component = "orchestrator-ecr"
    }
  )

}

inputs = {
  account_ids     = local.global_vars.locals.account_ids
  service_configs = local.global_vars.locals.service_configs
  tools_configs   = local.global_vars.locals.tools_configs
  lifecycle_policy_archive_after_not_pulled_days = 90
  lifecycle_policy_expire_after_archived_days = 365
  tags            = local.tags
}
