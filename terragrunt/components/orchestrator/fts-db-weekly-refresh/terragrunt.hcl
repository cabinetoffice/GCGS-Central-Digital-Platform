terraform {
  source = local.global_vars.locals.environment == "orchestrator" ? "../../../modules//orchestrator/fts-db-weekly-refresh" : null
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
      component = "fts-db-weekly-refresh"
    }
  )
}

dependency "core_iam" {
  config_path = "../../core/iam"
  mock_outputs = {
    ci_build_arn    = "mock"
    ci_pipeline_arn = "mock"
    terraform_arn   = "mock"
  }
}

dependency "core_networking" {
  config_path = "../../core/networking"
  mock_outputs = {
    private_subnet_ids = "mock"
    vpc_id             = "mock"
  }
}

dependency "core_security_groups" {
  config_path = "../../core/security-groups"
  mock_outputs = {
    ci_sg_id = "mock"
  }
}

dependency "orchestrator_ecr" {
  config_path = "../ecr"
  mock_outputs = {
    repository_urls = "mock"
  }
}

# fts_refresh_restore_mode options:
# - "main": delete and recreate the staging FTS cluster from the copied production snapshot.
# - "scratch": restore into a separate scratch cluster for testing without impacting staging services.
inputs = {
  account_ids                     = local.global_vars.locals.account_ids
  fts_refresh_restore_mode        = "main"
  fts_refresh_delete_main_cluster = true
  schedule_enabled                = false
  tags                            = local.tags

  ci_build_role_arn    = dependency.core_iam.outputs.ci_build_arn
  ci_pipeline_role_arn = dependency.core_iam.outputs.ci_pipeline_arn
  role_terraform_arn   = dependency.core_iam.outputs.terraform_arn

  private_subnet_ids = dependency.core_networking.outputs.private_subnet_ids
  vpc_id             = dependency.core_networking.outputs.vpc_id
  ci_sg_id           = dependency.core_security_groups.outputs.ci_sg_id

  repository_urls = dependency.orchestrator_ecr.outputs.repository_urls
}
