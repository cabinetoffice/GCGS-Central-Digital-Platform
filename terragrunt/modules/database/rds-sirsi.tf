module "cluster_sirsi" {
  source = "../db-postgres-cluster"

  backup_retention_period      = local.is_production ? 35 : 1
  db_name                      = local.sirsi_cluster_name
  db_sg_id                     = var.db_postgres_sg_id
  deletion_protection          = local.is_production
  engine_version               = var.aurora_postgres_engine_version
  family                       = "aurora-postgresql${floor(var.aurora_postgres_engine_version)}"
  monitoring_interval          = local.is_production ? 30 : 0
  monitoring_role_arn          = local.is_production ? var.role_rds_cloudwatch_arn : ""
  performance_insights_enabled = local.is_production
  instance_type                = var.aurora_postgres_instance_type
  db_parameters_instance       = { "max_connections" : 16000 }
  private_subnet_ids           = var.private_subnet_ids
  role_terraform_arn           = var.role_terraform_arn
  tags                         = var.tags
}
