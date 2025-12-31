terraform {
  source = local.global_vars.locals.environment != "orchestrator" ? "../../../modules//database" : null
}

include {
  path = find_in_parent_folders("root.hcl")
}

locals {
  global_vars  = read_terragrunt_config(find_in_parent_folders("root.hcl"))
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
    rds_backup_arn     = "mock"
    rds_cloudwatch_arn = "mock"
    terraform_arn      = "mock"
  }
}

dependency core_networking {
  config_path = "../../core/networking"
  mock_outputs = {
    private_subnets             = "mock"
    private_subnets_cidr_blocks = "mock"
    public_hosted_zone_id       = "mock"
    public_subnets              = "mock" # @TODO (ABN) burn me once migration is done
    vpc_id                      = "mock"
  }
}

dependency core_security_groups {
  config_path = "../../core/security-groups"
  mock_outputs = {
    db_mysql_sg_id    = "mock"
    db_postgres_sg_id = "mock"
    ec2_sg_id         = "mock"
  }
}

inputs = {
  aurora_postgres_instance_type    = local.global_vars.locals.aurora_postgres_instance_type
  aurora_postgres_instance_type_ev = local.global_vars.locals.aurora_postgres_instance_type_ev
  aurora_mysql_engine_version      = local.global_vars.locals.aurora_mysql_engine_version
  aurora_mysql_family              = local.global_vars.locals.aurora_mysql_family
  aurora_mysql_instance_type       = local.global_vars.locals.aurora_mysql_instance_type
  tags                             = local.tags

  private_subnet_ids          = dependency.core_networking.outputs.private_subnet_ids
  private_subnets_cidr_blocks = dependency.core_networking.outputs.private_subnets_cidr_blocks
  public_hosted_zone_id       = dependency.core_networking.outputs.public_hosted_zone_id
  public_subnet_ids           = dependency.core_networking.outputs.public_subnet_ids # @TODO (ABN) burn me once migration is done
  vpc_id                      = dependency.core_networking.outputs.vpc_id

  db_mysql_sg_id    = dependency.core_security_groups.outputs.db_mysql_sg_id
  db_postgres_sg_id = dependency.core_security_groups.outputs.db_postgres_sg_id
  ec2_sg_id         = dependency.core_security_groups.outputs.ec2_sg_id

  role_rds_backup_arn     = dependency.core_iam.outputs.rds_backup_arn
  role_rds_cloudwatch_arn = dependency.core_iam.outputs.rds_cloudwatch_arn
  role_terraform_arn      = dependency.core_iam.outputs.terraform_arn
}
