module "cluster_fts" {
  source = "../rds-cluster"

  apply_immediately            = true
  backup_retention_period      = var.is_production ? 14 : 1
  copy_tags_to_snapshot        = true
  db_name                      = local.fts_cluster_name
  db_parameters_cluster        = local.fts_db_parameters_cluster
  db_parameters_instance       = local.fts_db_parameters_instance
  db_sg_id                     = var.db_mysql_sg_id
  deletion_protection          = var.is_production
  engine_version               = var.aurora_mysql_engine_version
  family                       = var.aurora_mysql_family
  instance_type                = var.aurora_mysql_instance_type
  monitoring_interval          = 30
  monitoring_role_arn          = var.role_rds_cloudwatch_arn
  performance_insights_enabled = true
  private_subnet_ids           = var.private_subnet_ids
  public_subnet_ids            = var.environment == "development" ? var.public_subnet_ids : [] # @TODO (ABN) burn me once migration is done
  publicly_accessible          = true                                                          # @TODO (ABN) Disable after migration
  role_terraform_arn           = var.role_terraform_arn
  tags                         = var.tags
}
