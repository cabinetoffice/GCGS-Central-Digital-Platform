terraform {
  source = local.global_vars.locals.environment == "orchestrator" ? "../../../modules//orchestrator/canary" : null
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
      component = "canary"
    }
  )

}

dependency core_iam {
  config_path = "../../core/iam"
  mock_outputs = {
    canary_arn     = "mock"
    canary_name    = "mock"
    terraform_arn  = "mock"
    terraform_name = "mock"
  }
}

dependency core_networking {
  config_path = "../../core/networking"
  mock_outputs = {
    private_subnet_ids          = "mock"
    private_subnets_cidr_blocks = "mock"
    public_hosted_zone_fqdn     = "mock"
    public_hosted_zone_id       = "mock"
    public_subnet_ids           = "mock"
    public_subnets_cidr_blocks  = "mock"
    vpc_id                      = "mock"
  }
}

dependency core_security_groups {
  config_path = "../../core/security-groups"
  mock_outputs = {
    alb_sg_id                 = "mock"
    db_postgres_sg_id         = "mock"
    canary_sg_id              = "mock"
    vpce_s3_sg_id             = "mock"
    vpce_secretsmanager_sg_id = "mock"
  }
}

inputs = {

  account_ids            = local.global_vars.locals.account_ids
  pinned_service_version = local.global_vars.locals.pinned_service_version
  service_configs        = local.global_vars.locals.service_configs
  tags                   = local.tags

  role_canary_arn     = dependency.core_iam.outputs.canary_arn
  role_canary_name    = dependency.core_iam.outputs.canary_name
  role_terraform_arn  = dependency.core_iam.outputs.terraform_arn
  role_terraform_name = dependency.core_iam.outputs.terraform_name

  private_subnet_ids          = dependency.core_networking.outputs.private_subnet_ids
  private_subnets_cidr_blocks = dependency.core_networking.outputs.private_subnets_cidr_blocks
  public_hosted_zone_fqdn     = dependency.core_networking.outputs.public_hosted_zone_fqdn
  public_hosted_zone_id       = dependency.core_networking.outputs.public_hosted_zone_id
  public_subnet_ids           = dependency.core_networking.outputs.public_subnet_ids
  public_subnets_cidr_blocks  = dependency.core_networking.outputs.public_subnets_cidr_blocks
  vpc_id                      = dependency.core_networking.outputs.vpc_id
  vpc_cider                   = dependency.core_networking.outputs.vpc_cider


  alb_sg_id                 = dependency.core_security_groups.outputs.alb_sg_id
  canary_sg_id              = dependency.core_security_groups.outputs.canary_sg_id
  vpce_s3_sg_id             = dependency.core_security_groups.outputs.vpce_s3_sg_id
  vpce_secretsmanager_sg_id = dependency.core_security_groups.outputs.vpce_secretsmanager_sg_id
}
