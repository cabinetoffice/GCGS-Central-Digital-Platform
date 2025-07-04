# @todo (ABN) DP-1069 Check with Sammy if we still need to maintain this for FTS after recent Domain strategy changes
terraform {
  source = local.global_vars.locals.environment == "orchestrator" ? null : "../../../modules//external-networking"
}

include {
  path = find_in_parent_folders()
}

dependency core_networking {
  config_path = "../../core/networking"
  mock_outputs = {
    production_private_beta_hosted_zone_id = "mock"
    public_hosted_zone_id                  = "mock"
  }
}

locals {
  global_vars   = read_terragrunt_config(find_in_parent_folders("terragrunt.hcl"))
  external_vars = read_terragrunt_config(find_in_parent_folders("external.hcl"))

  tags = merge(
    local.global_vars.inputs.tags,
    local.external_vars.inputs.tags,
    {
      component = "networking"
    }
  )
}

inputs = {
  fts_azure_frontdoor = local.global_vars.locals.fts_azure_frontdoor
  tags                = local.tags

  core_hosted_zone_id = dependency.core_networking.outputs.public_hosted_zone_id
}
