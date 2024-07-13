terraform {
  source = local.global_vars.locals.environment != "orchestrator" ? "../../../modules//database" : null
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
      component = "database"
    }
  )

}

dependency core_iam {
  config_path = "../../core/iam"
  mock_outputs = {
    cloudwatch_events_arn            = "mock"
    cloudwatch_events_name           = "mock"
    db_connection_step_function_arn  = "mock"
    db_connection_step_function_name = "mock"
  }
}

dependency core_networking {
  config_path = "../../core/networking"
  mock_outputs = {
    private_subnets             = "mock"
    private_subnets_cidr_blocks = "mock"
    vpc_id                      = "mock"
  }
}

dependency core_security_group {
  config_path = "../../core/security-groups"
  mock_outputs = {
    db_postgres_sg_id = "mock"
  }
}

inputs = {
  tags = local.tags

  private_subnet_ids          = dependency.core_networking.outputs.private_subnet_ids
  private_subnets_cidr_blocks = dependency.core_networking.outputs.private_subnets_cidr_blocks
  vpc_id                      = dependency.core_networking.outputs.vpc_id

  db_postgres_sg_id = dependency.core_security_group.outputs.db_postgres_sg_id

  role_cloudwatch_events_arn            = dependency.core_iam.outputs.cloudwatch_events_arn
  role_cloudwatch_events_name           = dependency.core_iam.outputs.cloudwatch_events_name
  role_db_connection_step_function_arn  = dependency.core_iam.outputs.db_connection_step_function_arn
  role_db_connection_step_function_name = dependency.core_iam.outputs.db_connection_step_function_name
}
