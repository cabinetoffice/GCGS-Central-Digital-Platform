module "clamav_rest" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.tools_configs.clamav_rest.name}.json.tftpl",
    {
      account_id                     = data.aws_caller_identity.current.account_id
      container_port                 = var.tools_configs.clamav_rest.port
      cpu                            = var.is_production || var.environment == "staging" ? 4096 : var.tools_configs.clamav_rest.cpu
      environment                    = title(var.environment)
      host_port                      = var.tools_configs.clamav_rest.port
      image                          = "${local.orchestrator_account_id}.dkr.ecr.${data.aws_region.current.region}.amazonaws.com/cdp-${var.tools_configs.clamav_rest.name}:latest"
      lg_name                        = aws_cloudwatch_log_group.clamav_rest.name
      lg_prefix                      = "tools"
      lg_region                      = data.aws_region.current.region
      memory                         = var.is_production || var.environment == "staging" ? 8192 : var.tools_configs.clamav_rest.memory
      name                           = var.tools_configs.clamav_rest.name
      sqs_entity_verification_url    = var.sqs_entity_verification_url
      sqs_organisation_url           = var.sqs_organisation_url
      redis_auth_token_arn           = var.redis_auth_token_arn
      redis_port                     = var.redis_port
      redis_primary_endpoint_address = var.redis_primary_endpoint
    }
  )

  cluster_id             = var.ecs_cluster_id
  container_port         = var.tools_configs.clamav_rest.port
  cpu                    = var.is_production || var.environment == "staging" ? 4096 : var.tools_configs.clamav_rest.cpu
  desired_count          = var.is_production ? 9 : 2
  ecs_alb_sg_id          = var.alb_tools_sg_id
  ecs_listener_arn       = aws_lb_listener.tools.arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "tools"
  healthcheck_path       = "/"
  host_port              = var.tools_configs.clamav_rest.port
  memory                 = var.is_production || var.environment == "staging" ? 8192 : var.tools_configs.clamav_rest.memory
  name                   = var.tools_configs.clamav_rest.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
