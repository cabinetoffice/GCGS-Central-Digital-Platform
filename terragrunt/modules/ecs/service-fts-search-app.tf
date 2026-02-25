module "ecs_service_fts_app" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.fts_app.name}.json.tftpl",
    merge(
      local.fts_dotnet_fts_app,
      {
        container_port                 = var.service_configs.fts_app.port
        cpu                            = var.service_configs.fts_app.cpu
        host_port                      = var.service_configs.fts_app.port
        image                          = local.ecr_urls[var.service_configs.fts_app.name]
        lg_name                        = aws_cloudwatch_log_group.tasks[var.service_configs.fts_app.name].name
        lg_prefix                      = "app"
        memory                         = var.service_configs.fts_app.memory
        name                           = var.service_configs.fts_app.name
        redis_auth_token_arn           = var.redis_auth_token_arn
        redis_port                     = var.redis_port
        redis_primary_endpoint_address = var.redis_primary_endpoint
        ssm_data_protection_prefix     = local.ssm_data_protection_prefix
      }
    )
  )

  cluster_id             = local.fts_cluster_id
  container_port         = var.service_configs.fts_app.port
  cpu                    = var.service_configs.fts_app.cpu
  desired_count          = var.service_configs.fts_app.desired_count
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = local.fts_ecs_listener_arn
  ecs_service_base_sg_id = var.ecs_sg_id
  extra_host_headers     = var.fts_extra_host_headers
  family                 = "app"
  host_port              = var.service_configs.fts_app.port
  listener_name          = "fts-${var.service_configs.fts_app.name}"
  memory                 = var.service_configs.fts_app.memory
  name                   = var.service_configs.fts_app.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
