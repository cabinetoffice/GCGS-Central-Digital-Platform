module "ecs_service_healthcheck" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/healthcheck.json.tftpl",
    {
      account_id             = data.aws_caller_identity.current.account_id
      container_port         = var.healthcheck_config.port
      cpu                    = var.healthcheck_config.cpu
      db_address             = var.db_address
      db_name                = var.db_name
      db_username            = "${var.db_credentials}:username::"
      environment            = local.service_environment
      host_port              = var.healthcheck_config.port
      image                  = "${local.orchestrator_account_id}.dkr.ecr.${data.aws_region.current.name}.amazonaws.com/cdp-healthcheck:latest"
      lg_name                = aws_cloudwatch_log_group.healthcheck.name
      lg_prefix              = "tools"
      lg_region              = data.aws_region.current.name
      memory                 = var.healthcheck_config.memory
      name                   = var.healthcheck_config.name
      queue_entity_verification_queue_url = var.queue_entity_verification_queue_url
      queue_organisation_queue_url        = var.queue_organisation_queue_url
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
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
