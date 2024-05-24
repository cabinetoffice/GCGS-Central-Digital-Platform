terraform {
  source = "../../../modules//database"
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
}
