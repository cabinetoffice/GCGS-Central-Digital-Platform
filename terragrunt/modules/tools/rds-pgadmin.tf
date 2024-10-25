module "rds_pgadmin" {
  source = "../rds"

  create_read_replica          = false
  backup_retention_period      = 0
  db_name                      = "${local.name_prefix}-pgadmin"
  db_postgres_sg_id            = var.db_postgres_sg_id
  deletion_protection          = var.is_production
  environment                  = var.environment
  max_allocated_storage        = 0
  monitoring_interval          = var.is_production ? 30 : 0
  monitoring_role_arn          = var.is_production ? var.role_rds_cloudwatch_arn : ""
  multi_az                     = false
  performance_insights_enabled = var.is_production
  postgres_engine_version      = var.postgres_engine_version
  postgres_instance_type       = "db.t4g.micro"
  private_subnet_ids           = var.private_subnet_ids
  role_terraform_arn           = var.role_terraform_arn
  tags                         = var.tags
}
