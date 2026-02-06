module "ecs_service_opensearch_admin" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.opensearch_admin_config.name}.json.tftpl",
    {
      account_id          = data.aws_caller_identity.current.account_id
      container_port      = var.opensearch_admin_config.port
      cpu                 = var.opensearch_admin_config.cpu
      environment         = title(var.environment)
      host_port           = var.opensearch_admin_config.port
      image               = "public.ecr.aws/aws-observability/aws-sigv4-proxy:1.11"
      lg_name             = aws_cloudwatch_log_group.opensearch_admin.name
      lg_prefix           = "tools"
      lg_region           = data.aws_region.current.region
      memory              = var.opensearch_admin_config.memory
      name                = var.opensearch_admin_config.name
      opensearch_endpoint = var.opensearch_endpoint
    }
  )

  cluster_id             = var.ecs_cluster_id
  container_port         = var.opensearch_admin_config.port
  cpu                    = var.opensearch_admin_config.cpu
  desired_count          = var.environment == "development" ? 1 : 0
  ecs_alb_sg_id          = var.alb_tools_sg_id
  ecs_listener_arn       = aws_lb_listener.tools.arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "tools"
  healthcheck_path       = "/" #"/_cluster/health?local=true
  healthcheck_matcher    = "200-499"
  host_port              = var.opensearch_admin_config.port
  memory                 = var.opensearch_admin_config.memory
  name                   = var.opensearch_admin_config.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_opensearch_admin_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  user_pool_arn          = var.user_pool_arn_opensearch_admin
  user_pool_client_id    = var.user_pool_client_id_opensearch_admin
  user_pool_domain       = var.user_pool_domain_opensearch_admin
  vpc_id                 = var.vpc_id
}
