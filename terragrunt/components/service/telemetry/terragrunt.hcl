terraform {
  source = "../../../modules//telemetry"
}

include {
  path = find_in_parent_folders()
}

locals {
  global_vars  = read_terragrunt_config(find_in_parent_folders("terragrunt.hcl"))
  service_vars = read_terragrunt_config(find_in_parent_folders("service.hcl"))

  tags = merge(
    local.global_vars.inputs.tags,
    local.service_vars.inputs.tags,
    {
      component = "telemetry"
    }
  )

}

inputs = {
  service_configs = local.global_vars.locals.service_configs
  tags            = local.tags
}
