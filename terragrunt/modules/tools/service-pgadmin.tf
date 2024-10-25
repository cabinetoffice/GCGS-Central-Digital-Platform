module "ecs_service_pgadmin" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/pgadmin.json.tftpl",
    {
      account_id                      = data.aws_caller_identity.current.account_id
      container_port                  = var.pgadmin_config.port
      cpu                             = var.pgadmin_config.cpu
      db_entity_verification_address  = var.db_entity_verification_address
      db_entity_verification_name     = var.db_entity_verification_name
      db_entity_verification_username = "${var.db_entity_verification_credentials_arn}:username::"
      db_sirsi_address                = var.db_sirsi_address
      db_sirsi_name                   = var.db_sirsi_name
      db_sirsi_username               = "${var.db_sirsi_credentials_arn}:username::"
      pgadmin_admin_password          = "${aws_secretsmanager_secret.pgadmin_credentials.arn}:ADMIN_PASSWORD::"
      pgadmin_admin_user              = "${aws_secretsmanager_secret.pgadmin_credentials.arn}:ADMIN_USERNAME::"
      pgadmin_database_host           = module.rds_pgadmin.db_address
      pgadmin_database_name           = module.rds_pgadmin.db_name
      pgadmin_database_password       = "${module.rds_pgadmin.db_credentials_arn}:password::"
      pgadmin_database_username       = "${module.rds_pgadmin.db_credentials_arn}:username::"
      host_port                       = var.pgadmin_config.port
      image                           = "${local.orchestrator_account_id}.dkr.ecr.${data.aws_region.current.name}.amazonaws.com/cdp-pgadmin:8.12.0"
      lg_name                         = aws_cloudwatch_log_group.pgadmin.name
      lg_prefix                       = "tools"
      lg_region                       = data.aws_region.current.name
      memory                          = var.pgadmin_config.memory
      name                            = var.pgadmin_config.name
    }
  )

  cluster_id                    = var.ecs_cluster_id
  container_port                = var.pgadmin_config.port
  cpu                           = var.pgadmin_config.cpu
  ecs_alb_sg_id                 = var.ecs_alb_sg_id
  ecs_listener_arn              = var.ecs_listener_arn
  ecs_service_base_sg_id        = var.ecs_sg_id
  family                        = "tools"
  healthcheck_healthy_threshold = 5
  healthcheck_interval          = 60
  healthcheck_path              = "/login"
  healthcheck_timeout           = 30
  host_port                     = var.pgadmin_config.port
  memory                        = var.pgadmin_config.memory
  name                          = var.pgadmin_config.name
  private_subnet_ids            = var.private_subnet_ids
  product                       = var.product
  public_domain                 = var.public_domain
  role_ecs_task_arn             = var.role_ecs_task_arn
  role_ecs_task_exec_arn        = var.role_ecs_task_exec_arn
  tags                          = var.tags
  vpc_id                        = var.vpc_id
}
