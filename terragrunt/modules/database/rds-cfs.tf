module "cluster_cfs" {
  source = "../rds-cluster"

  apply_immediately            = true
  backup_retention_period      = var.is_production ? 14 : 1
  copy_tags_to_snapshot        = true
  db_name                      = local.cfs_cluster_name
  db_parameters_cluster        = local.cfs_db_parameters_cluster
  db_parameters_instance       = local.cfs_db_parameters_instance
  db_sg_id                     = var.db_mysql_sg_id
  deletion_protection          = var.is_production
  engine_version               = var.aurora_mysql_engine_version
  family                       = var.aurora_mysql_family
  instance_type                = var.aurora_mysql_instance_type
  instance_count               = local.cfs_instance_count
  monitoring_interval          = 30
  monitoring_role_arn          = var.role_rds_cloudwatch_arn
  performance_insights_enabled = true
  private_subnet_ids           = var.private_subnet_ids
  role_terraform_arn           = var.role_terraform_arn
  tags                         = var.tags
}
