module "ecs_service_opensearch_gateway" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/opensearch_gateway.json.tftpl",
    {
      account_id          = data.aws_caller_identity.current.account_id
      container_port      = var.opensearch_gateway_config.port
      cpu                 = var.opensearch_gateway_config.cpu
      environment         = title(var.environment)
      host_port           = var.opensearch_gateway_config.port
      image               = "public.ecr.aws/aws-observability/aws-sigv4-proxy:1.11"
      lg_name             = aws_cloudwatch_log_group.opensearch_gateway.name
      lg_prefix           = "tools"
      lg_region           = data.aws_region.current.region
      memory              = var.opensearch_gateway_config.memory
      name                = var.opensearch_gateway_config.name
      opensearch_endpoint = var.opensearch_endpoint
    }
  )

  cluster_id             = var.ecs_cluster_id
  container_port         = var.opensearch_gateway_config.port
  cpu                    = var.opensearch_gateway_config.cpu
  desired_count          = 0
  ecs_alb_sg_id          = var.alb_tools_sg_id
  ecs_listener_arn       = aws_lb_listener.tools.arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "tools"
  healthcheck_path       = "/" #"/_cluster/health?local=true
  host_port              = var.opensearch_gateway_config.port
  memory                 = var.opensearch_gateway_config.memory
  name                   = var.opensearch_gateway_config.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  # user_pool_arn           = var.user_pool_arn_opensearch_gateway
  # user_pool_client_id     = var.user_pool_client_id_opensearch_gateway
  # user_pool_domain        = var.user_pool_domain_opensearch_gateway
  vpc_id = var.vpc_id
}
