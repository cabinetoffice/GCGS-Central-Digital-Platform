terraform {
  source = local.global_vars.locals.environment == "orchestrator" ? "../../../modules//orchestrator/notification" : null
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
      component = "notification"
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
    notification_role_arn  = "mock"
    notification_role_name = "mock"
    cloudwatch_events_arn  = "mock"
    cloudwatch_events_name = "mock"
  }
}

dependency orchestrator_ci {
  config_path = "../ci"
  mock_outputs = {
    deployment_pipeline_name                   = "mock"
    event_rule_ci_service_version_updated_name = "mock"
    ssm_service_version_arn                    = "mock"
    ssm_service_version_name                   = "mock"
  }
}

dependency orchestrator_ecr {
  config_path = "../ecr"
  mock_outputs = {
    repository_urls = "mock"
  }
}


inputs = {
  account_ids = local.global_vars.locals.account_ids
  tags        = local.tags

  role_notification_step_function_arn  = dependency.core_iam.outputs.notification_step_function_arn
  role_notification_step_function_name = dependency.core_iam.outputs.notification_step_function_name
  role_cloudwatch_events_arn           = dependency.core_iam.outputs.cloudwatch_events_arn
  role_cloudwatch_events_name          = dependency.core_iam.outputs.cloudwatch_events_name

  repository_urls = dependency.orchestrator_ecr.outputs.repository_urls

  deployment_pipeline_name                   = dependency.orchestrator_ci.outputs.deployment_pipeline_name
  event_rule_ci_service_version_updated_name = dependency.orchestrator_ci.outputs.event_rule_ci_service_version_updated_name
  ssm_service_version_arn                    = dependency.orchestrator_ci.outputs.ssm_service_version_arn
  ssm_service_version_name                   = dependency.orchestrator_ci.outputs.ssm_service_version_name
}
