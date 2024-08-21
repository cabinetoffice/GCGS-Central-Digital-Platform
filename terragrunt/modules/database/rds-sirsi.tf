module "rds_sirsi" {
  source = "../rds"

  db_name                               = local.name_prefix
  db_postgres_sg_id                     = var.db_postgres_sg_id
  environment                           = var.environment
  postgres_engine_version               = var.postgres_engine_version
  postgres_instance_type                = var.postgres_instance_type
  private_subnet_ids                    = var.private_subnet_ids
  role_terraform_arn                    = var.role_terraform_arn
  tags                                  = var.tags
}
