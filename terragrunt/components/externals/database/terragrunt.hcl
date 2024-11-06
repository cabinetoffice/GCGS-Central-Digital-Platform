terraform {
  source = local.global_vars.locals.environment == "integration" ? "../../../modules//externals/database" : null
}

include {
  path = find_in_parent_folders()
}

locals {
  global_vars = read_terragrunt_config(find_in_parent_folders("terragrunt.hcl"))
  externals_vars = read_terragrunt_config(find_in_parent_folders("externals.hcl"))

  tags = merge(
    local.global_vars.inputs.tags,
    local.externals_vars.inputs.tags,
    {
      component = "database"
    }
  )

}

dependency core_iam {
  config_path = "../../core/iam"
  mock_outputs = {
    rds_cloudwatch_arn = "mock"
    terraform_arn      = "mock"
  }
}

dependency externals_networking {
  config_path = "../networking"
  mock_outputs = {
    private_subnet_ids          = "mock"
    private_subnets_cidr_blocks = "mock"
    vpc_id                      = "mock"
  }
}

dependency externals_security_groups {
  config_path = "../security-groups"
  mock_outputs = {
    db_mysql_sg_id = "mock"
  }
}

inputs = {
  tags                = local.tags

  private_subnet_ids          = dependency.externals_networking.outputs.private_subnet_ids
  private_subnets_cidr_blocks = dependency.externals_networking.outputs.private_subnets_cidr_blocks
  vpc_id                      = dependency.externals_networking.outputs.vpc_id

  db_mysql_sg_id = dependency.externals_security_groups.outputs.db_mysql_sg_id

  role_terraform_arn      = dependency.core_iam.outputs.terraform_arn
}
