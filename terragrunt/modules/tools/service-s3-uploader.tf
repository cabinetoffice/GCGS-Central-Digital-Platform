module "ecs_service_s3_uploader" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.s3_uploader_config.name}.json.tftpl",
    {
      account_id          = data.aws_caller_identity.current.account_id
      container_port      = var.s3_uploader_config.port
      cpu                 = var.s3_uploader_config.cpu
      environment         = title(var.environment)
      host_port           = var.s3_uploader_config.port
      image               = "${local.orchestrator_account_id}.dkr.ecr.${data.aws_region.current.region}.amazonaws.com/cdp-${var.tools_configs.s3_uploader.name}:latest"
      lg_name             = aws_cloudwatch_log_group.s3_uploader.name
      lg_prefix           = "tools"
      lg_region           = data.aws_region.current.region
      memory              = var.s3_uploader_config.memory
      name                = var.s3_uploader_config.name
      opensearch_endpoint = var.opensearch_endpoint
      s3_bucket           = var.s3_fts_bucket
    }
  )

  cluster_id             = var.ecs_cluster_id
  container_port         = var.s3_uploader_config.port
  cpu                    = var.s3_uploader_config.cpu
  desired_count          = var.environment == "development" ? 1 : 0
  ecs_alb_sg_id          = var.alb_tools_sg_id
  ecs_listener_arn       = aws_lb_listener.tools.arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "tools"
  healthcheck_path       = "/healthz"
  host_port              = var.s3_uploader_config.port
  memory                 = var.s3_uploader_config.memory
  name                   = var.s3_uploader_config.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  # user_pool_arn           = var.user_pool_arn_s3_uploader
  # user_pool_client_id     = var.user_pool_client_id_s3_uploader
  # user_pool_domain        = var.user_pool_domain_s3_uploader
  vpc_id = var.vpc_id
}
