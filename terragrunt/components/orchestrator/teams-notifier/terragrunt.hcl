terraform {
  source = local.global_vars.locals.environment == "orchestrator" ? "../../../modules//orchestrator/teams-notifier" : null
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
      component = "teams-notifier"
    }
  )
}

inputs = {
  tags = local.tags
}
