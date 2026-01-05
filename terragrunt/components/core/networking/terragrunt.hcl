terraform {
  source = "../../../modules//core-networking"
}

include {
  path = find_in_parent_folders("root.hcl")
}

dependency core_iam {
  config_path = "../../core/iam"
  mock_outputs = {
    terraform_arn  = "mock"
    terraform_name = "mock"
  }
}

locals {
  global_vars = read_terragrunt_config(find_in_parent_folders("root.hcl"))
  core_vars   = read_terragrunt_config(find_in_parent_folders("core.hcl"))

  tags = merge(
    local.global_vars.inputs.tags,
    local.core_vars.inputs.tags,
    {
      component = "networking"
    }
  )
}

inputs = {
  tags = local.tags
}
