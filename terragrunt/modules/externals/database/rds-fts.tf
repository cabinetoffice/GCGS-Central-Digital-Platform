module "rds_fts" {
  source = "../../rds-cluster"

  backup_retention_period      = var.is_production ? 14 : 1
  copy_tags_to_snapshot        = true
  db_name                      = local.name_prefix
  db_parameters_cluster        = local.db_parameters_cluster
  db_parameters_instance       = local.db_parameters_instance
  db_sg_id                     = var.db_mysql_sg_id
  deletion_protection          = var.is_production
  engine_version               = "8.0.mysql_aurora.3.07.1"
  family                       = "aurora-mysql8.0"
  instance_type                = "db.r5.large"
  monitoring_interval          = var.is_production ? 30 : 0
  monitoring_role_arn          = var.is_production ? var.role_rds_cloudwatch_arn : ""
  performance_insights_enabled = var.is_production
  private_subnet_ids           = var.private_subnet_ids
  publicly_accessible          = true
  role_terraform_arn           = var.role_terraform_arn
  tags                         = var.tags
}
