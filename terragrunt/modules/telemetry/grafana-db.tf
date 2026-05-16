module "grafana_db" {
  source = "../rds"

  db_name              = "${local.name_prefix}-grafana"
  db_sg_id             = var.db_postgres_sg_id
  engine               = "postgres"
  engine_version       = "16.6"
  family               = "postgres16"
  instance_type        = local.grafana_db_instance_type
  multi_az             = local.grafana_db_multi_az
  parameter_group_name = "${local.name_prefix}-grafana"
  private_subnet_ids   = var.private_subnet_ids
  publicly_accessible  = false
  performance_insights_enabled = false

  role_terraform_arn = var.role_terraform_arn
  tags               = var.tags
}
