terraform {
  source = "../../../modules//networking"
}

include {
  path = find_in_parent_folders()
}

locals {
  global_vars = read_terragrunt_config(find_in_parent_folders("terragrunt.hcl"))
  common_vars = read_terragrunt_config(find_in_parent_folders("common.hcl"))

  tags = merge(
    local.global_vars.inputs.tags,
    local.common_vars.inputs.tags,
    {
      component = "networking"
    }
  )
}

dependency core_networking {
  config_path = "../../core/networking"
  mock_outputs = {
    private_route_table_ids = ["mock"]
    private_subnets         = "mock"
    public_route_table_ids  = ["mock"]
    vpc_id                  = "mock"
  }
}

dependency core_security_groups {
  config_path = "../../core/security-groups"
  mock_outputs = {
    vpce_ecr_api_sg_id        = "mock"
    vpce_ecr_dkr_sg_id        = "mock"
    vpce_logs_sg_id           = "mock"
    vpce_s3_sg_id             = "mock"
    vpce_secretsmanager_sg_id = "mock"
  }
}

inputs = {
  tags = local.tags

  private_route_table_ids = dependency.core_networking.outputs.private_route_table_ids
  private_subnet_ids      = dependency.core_networking.outputs.private_subnet_ids
  public_route_table_ids  = dependency.core_networking.outputs.public_route_table_ids
  public_subnet_ids       = dependency.core_networking.outputs.public_subnet_ids
  vpc_id                  = dependency.core_networking.outputs.vpc_id

  vpce_ecr_api_sg_id        = dependency.core_security_groups.outputs.vpce_ecr_api_sg_id
  vpce_ecr_dkr_sg_id        = dependency.core_security_groups.outputs.vpce_ecr_dkr_sg_id
  vpce_logs_sg_id           = dependency.core_security_groups.outputs.vpce_logs_sg_id
  vpce_s3_sg_id             = dependency.core_security_groups.outputs.vpce_s3_sg_id
  vpce_secretsmanager_sg_id = dependency.core_security_groups.outputs.vpce_secretsmanager_sg_id
}
