terraform {
  source = local.global_vars.locals.environment == "orchestrator" ? "../../../modules//orchestrator/common" : null
}

include {
  path = find_in_parent_folders("root.hcl")
}

locals {
  global_vars       = read_terragrunt_config(find_in_parent_folders("root.hcl"))
  orchestrator_vars = read_terragrunt_config(find_in_parent_folders("orchestrator.hcl"))

  exclude_list = ["orchestrator"]

  pinned_service_versions_cfs = {
    for key, env in local.global_vars.locals.environments :
    key => try(env.pinned_service_version_cfs, null) if !(contains(local.exclude_list, env.name))
  }

  pinned_service_versions_fts = {
    for key, env in local.global_vars.locals.environments :
    key => try(env.pinned_service_version_fts, null) if !(contains(local.exclude_list, env.name))
  }

  pinned_service_versions_sirsi = {
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

  pinned_service_versions_cfs   = local.pinned_service_versions_cfs
  pinned_service_versions_fts   = local.pinned_service_versions_fts
  pinned_service_versions_sirsi = local.pinned_service_versions_sirsi
  tags                          = local.tags
}
