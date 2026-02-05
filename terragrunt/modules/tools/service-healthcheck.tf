module "ecs_service_healthcheck" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.healthcheck_config.name}.json.tftpl",
    {
      account_id                      = data.aws_caller_identity.current.account_id
      container_port                  = var.healthcheck_config.port
      cpu                             = var.healthcheck_config.cpu
      db_entity_verification_address  = var.db_ev_cluster_address
      db_entity_verification_name     = var.db_ev_cluster_name
      db_entity_verification_port     = var.db_entity_verification_cluster_port
      db_entity_verification_username = "${var.db_ev_cluster_credentials_arn}:username::"
      db_sirsi_address                = var.db_sirsi_cluster_address
      db_sirsi_name                   = var.db_sirsi_cluster_name
      db_sirsi_port                   = var.db_sirsi_cluster_port
      db_sirsi_username               = "${var.db_sirsi_cluster_credentials_arn}:username::"
      environment                     = title(var.environment)
      host_port                       = var.healthcheck_config.port
      image                           = "${local.orchestrator_account_id}.dkr.ecr.${data.aws_region.current.region}.amazonaws.com/cdp-healthcheck:1.0.1"
      lg_name                         = aws_cloudwatch_log_group.healthcheck.name
      lg_prefix                       = "tools"
      lg_region                       = data.aws_region.current.region
      memory                          = var.healthcheck_config.memory
      name                            = var.healthcheck_config.name
      sqs_entity_verification_url     = var.sqs_entity_verification_url
      sqs_organisation_url            = var.sqs_organisation_url
      redis_auth_token_arn            = var.redis_auth_token_arn
      redis_port                      = var.redis_port
      redis_primary_endpoint_address  = var.redis_primary_endpoint
    }
  )

  cluster_id             = var.ecs_cluster_id
  container_port         = var.healthcheck_config.port
  cpu                    = var.healthcheck_config.cpu
  ecs_alb_sg_id          = var.alb_tools_sg_id
  ecs_listener_arn       = aws_lb_listener.tools.arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "tools"
  healthcheck_path       = "/healthz"
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
