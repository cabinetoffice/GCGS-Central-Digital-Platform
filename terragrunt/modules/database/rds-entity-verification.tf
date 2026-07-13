module "cluster_entity_verification" {
  source = "../db-postgres-cluster"

  apply_immediately            = local.is_staging
  backup_retention_period      = local.is_production ? 35 : 1
  db_name                      = local.ev_cluster_name
  db_sg_id                     = var.db_postgres_sg_id
  deletion_protection          = local.is_production
  engine_version               = var.aurora_postgres_engine_version
  family                       = "aurora-postgresql${floor(var.aurora_postgres_engine_version)}"
  monitoring_interval          = local.is_production ? 30 : 0
  monitoring_role_arn          = local.is_production ? var.role_rds_cloudwatch_arn : ""
  performance_insights_enabled = local.is_production
  instance_type                = var.aurora_postgres_instance_type_ev
  private_subnet_ids           = var.private_subnet_ids
  restore_from_snapshot        = var.ev_restore_from_snapshot
  role_terraform_arn           = var.role_terraform_arn
  snapshot_identifier          = var.ev_snapshot_identifier
  tags                         = var.tags
}
