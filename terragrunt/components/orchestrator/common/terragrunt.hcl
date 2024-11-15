terraform {
  source = local.global_vars.locals.environment == "orchestrator" ? "../../../modules//orchestrator/common" : null
}

include {
  path = find_in_parent_folders()
}

locals {
  global_vars = read_terragrunt_config(find_in_parent_folders("terragrunt.hcl"))
  orchestrator_vars = read_terragrunt_config(find_in_parent_folders("orchestrator.hcl"))

  exclude_list = ["orchestrator"]

  pinned_service_versions = {
    for key, env in local.global_vars.locals.environments :
    key => try(env.pinned_service_version, null) if !(contains(local.exclude_list, env.name))
  }

  tags = merge(
    local.global_vars.inputs.tags,
    local.orchestrator_vars.inputs.tags,
    {
      component = "common"
    }
  )

}

inputs = {

  pinned_service_versions = local.pinned_service_versions
  tags                    = local.tags
}
