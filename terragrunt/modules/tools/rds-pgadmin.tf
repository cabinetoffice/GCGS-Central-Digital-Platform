module "rds_pgadmin" {
  source = "../rds"

  backup_retention_period      = 0
  create_read_replica          = false
  db_name                      = "${local.name_prefix}-pgadmin"
  db_sg_id                     = var.db_postgres_sg_id
  deletion_protection          = var.is_production
  engine_version               = var.postgres_engine_version
  family                       = "postgres${floor(var.postgres_engine_version)}"
  instance_type                = "db.t4g.micro"
  max_allocated_storage        = 0
  monitoring_interval          = var.is_production ? 30 : 0
  monitoring_role_arn          = var.is_production ? var.role_rds_cloudwatch_arn : ""
  multi_az                     = false
  parameter_group_name         = "${local.name_prefix}-pgadmin-${floor(var.postgres_engine_version)}"
  performance_insights_enabled = var.is_production
  private_subnet_ids           = var.private_subnet_ids
  role_terraform_arn           = var.role_terraform_arn
  tags                         = var.tags
}
