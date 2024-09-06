module "rds_sirsi" {
  source = "../rds"

  create_read_replica          = local.is_production
  backup_retention_period      = local.is_production ? 14 : 0
  db_name                      = local.name_prefix
  db_postgres_sg_id            = var.db_postgres_sg_id
  deletion_protection          = local.is_production
  environment                  = var.environment
  max_allocated_storage        = local.is_production ? 50 : 0
  monitoring_interval          = local.is_production ? 30 : 0
  monitoring_role_arn          = local.is_production ? var.role_rds_cloudwatch_arn : ""
  multi_az                     = local.is_production
  performance_insights_enabled = local.is_production
  postgres_engine_version      = var.postgres_engine_version
  postgres_instance_type       = var.postgres_instance_type
  private_subnet_ids           = var.private_subnet_ids
  role_terraform_arn           = var.role_terraform_arn
  tags                         = var.tags
}
