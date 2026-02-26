module "ecs_service_fts_search_api" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.fts_search_api.name}.json.tftpl",
    merge(
      local.fts_dotnet_search_api,
      {
        cpu          = var.service_configs.fts_search_api.cpu
        image        = local.ecr_urls[var.service_configs.fts_search_api.name]
        lg_name      = aws_cloudwatch_log_group.tasks[var.service_configs.fts_search_api.name].name
        memory       = var.service_configs.fts_search_api.memory
        name         = var.service_configs.fts_search_api.name
        service_port = local.service_port_by_cluster[var.service_configs.fts_search_api.cluster]
      }
    )
  )

  cluster_id             = local.fts_cluster_id
  cpu                    = var.service_configs.fts_search_api.cpu
  desired_count          = var.service_configs.fts_search_api.desired_count
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = local.fts_ecs_listener_arn
  ecs_service_base_sg_id = var.ecs_sg_id
  extra_host_headers     = var.fts_extra_host_headers
  family                 = "app"
  listener_name          = "dotnet-${var.service_configs.fts_search_api.name}"
  listener_priority      = try(var.service_configs.fts_search_api.listener_priority, null)
  memory                 = var.service_configs.fts_search_api.memory
  name                   = var.service_configs.fts_search_api.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  service_port           = local.service_port_by_cluster[var.service_configs.fts_search_api.cluster]
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
