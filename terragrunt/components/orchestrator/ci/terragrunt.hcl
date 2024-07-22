terraform {
  source = local.global_vars.locals.environment == "orchestrator" ? "../../../modules//orchestrator/ci" : null
}

include {
  path = find_in_parent_folders()
}

locals {
  global_vars = read_terragrunt_config(find_in_parent_folders("terragrunt.hcl"))
  orchestrator_vars = read_terragrunt_config(find_in_parent_folders("orchestrator.hcl"))

  tags = merge(
    local.global_vars.inputs.tags,
    local.orchestrator_vars.inputs.tags,
    {
      component = "ci"
    }
  )
}

dependency common_networking {
  config_path = "../../common/networking"
  mock_outputs = {
    vpce_s3_prefix_list_id = "mock"
  }
}

dependency core_iam {
  config_path = "../../core/iam"
  mock_outputs = {
    ci_role_arn            = "mock"
    ci_role_name           = "mock"
    ci_build_arn           = "mock"
    ci_build_name          = "mock"
    ci_pipeline_arn        = "mock"
    ci_pipeline_name       = "mock"
    cloudwatch_events_arn  = "mock"
    cloudwatch_events_name = "mock"
  }
}

dependency core_networking {
  config_path = "../../core/networking"
  mock_outputs = {
    private_subnet_ids = "mock"
    vpc_id             = "mock"
  }
}

dependency core_security_groups {
  config_path = "../../core/security-groups"
  mock_outputs = {
    ci_sg_id                  = "mock"
    vpce_ecr_api_sg_id        = "mock"
    vpce_ecr_dkr_sg_id        = "mock"
    vpce_s3_sg_id             = "mock"
    vpce_secretsmanager_sg_id = "mock"
  }
}


inputs = {
  account_ids         = local.global_vars.locals.account_ids
  tags                = local.tags
  tfstate_bucket_name = local.global_vars.locals.tg.state_bucket

  ci_role_arn                 = dependency.core_iam.outputs.terraform_arn
  ci_role_name                = dependency.core_iam.outputs.terraform_name
  ci_build_role_arn           = dependency.core_iam.outputs.ci_build_arn
  ci_build_role_name          = dependency.core_iam.outputs.ci_build_name
  ci_pipeline_role_arn        = dependency.core_iam.outputs.ci_pipeline_arn
  ci_pipeline_role_name       = dependency.core_iam.outputs.ci_pipeline_name
  role_cloudwatch_events_arn  = dependency.core_iam.outputs.cloudwatch_events_arn
  role_cloudwatch_events_name = dependency.core_iam.outputs.cloudwatch_events_name

  vpce_s3_prefix_list_id = dependency.common_networking.outputs.vpce_s3_prefix_list_id

  private_subnet_ids = dependency.core_networking.outputs.private_subnet_ids
  vpc_id             = dependency.core_networking.outputs.vpc_id

  ci_sg_id                  = dependency.core_security_groups.outputs.ci_sg_id
  vpce_ecr_api_sg_id        = dependency.core_security_groups.outputs.vpce_ecr_api_sg_id
  vpce_ecr_dkr_sg_id        = dependency.core_security_groups.outputs.vpce_ecr_dkr_sg_id
  vpce_logs_sg_id           = dependency.core_security_groups.outputs.vpce_logs_sg_id
  vpce_s3_sg_id             = dependency.core_security_groups.outputs.vpce_s3_sg_id
  vpce_secretsmanager_sg_id = dependency.core_security_groups.outputs.vpce_secretsmanager_sg_id
}
