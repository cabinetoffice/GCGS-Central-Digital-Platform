module "rds_entity_verification" {
  source = "../rds"

  db_name                               = "${local.name_prefix}-entity-verification"
  db_postgres_sg_id                     = var.db_postgres_sg_id
  environment                           = var.environment
  postgres_engine_version               = var.postgres_engine_version
  postgres_instance_type                = var.postgres_instance_type
  private_subnet_ids                    = var.private_subnet_ids
  role_cloudwatch_events_arn            = var.role_cloudwatch_events_arn
  role_cloudwatch_events_name           = var.role_cloudwatch_events_name
  role_db_connection_step_function_arn  = var.role_db_connection_step_function_arn
  role_db_connection_step_function_name = var.role_db_connection_step_function_name
  role_terraform_arn                    = var.role_terraform_arn
  tags                                  = var.tags
}
