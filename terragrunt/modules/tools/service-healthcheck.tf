module "ecs_service_healthcheck" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/healthcheck.json.tftpl",
    {
      account_id                      = data.aws_caller_identity.current.account_id
      container_port                  = var.healthcheck_config.port
      cpu                             = var.healthcheck_config.cpu
      db_entity_verification_address  = var.db_entity_verification_address
      db_entity_verification_name     = var.db_entity_verification_name
      db_entity_verification_username = "${var.db_entity_verification_credentials_arn}:username::"
      db_sirsi_address                = var.db_sirsi_address
      db_sirsi_name                   = var.db_sirsi_name
      db_sirsi_username               = "${var.db_sirsi_credentials_arn}:username::"
      environment                     = title(var.environment)
      host_port                       = var.healthcheck_config.port
      image                           = "${local.orchestrator_account_id}.dkr.ecr.${data.aws_region.current.name}.amazonaws.com/cdp-healthcheck:latest"
      lg_name                         = aws_cloudwatch_log_group.healthcheck.name
      lg_prefix                       = "tools"
      lg_region                       = data.aws_region.current.name
      memory                          = var.healthcheck_config.memory
      name                            = var.healthcheck_config.name
      queue_healthcheck_queue_url     = var.queue_healthcheck_queue_url
    }
  )

  cluster_id             = var.ecs_cluster_id
  container_port         = var.healthcheck_config.port
  cpu                    = var.healthcheck_config.cpu
  ecs_alb_sg_id          = var.ecs_alb_sg_id
  ecs_listener_arn       = var.ecs_listener_arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "tools"
  healthcheck_path       = "/health"
  host_port              = var.healthcheck_config.port
  memory                 = var.healthcheck_config.memory
  name                   = var.healthcheck_config.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  user_pool_arn          = var.user_pool_arn_healthcheck
  user_pool_client_id    = var.user_pool_client_id_healthcheck
  user_pool_domain       = var.user_pool_domain_healthcheck
  vpc_id                 = var.vpc_id
}
