module "ecs_service_fts_search_indexer" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.fts_search_indexer.name}.json.tftpl",
    merge(
      local.fts_dotnet_search_indexer,
      {
        cpu    = var.service_configs.fts_search_indexer.cpu
        image  = local.ecr_urls[var.service_configs.fts_search_indexer.name]
        lg_name = aws_cloudwatch_log_group.tasks[var.service_configs.fts_search_indexer.name].name
        memory = var.service_configs.fts_search_indexer.memory
        name   = var.service_configs.fts_search_indexer.name
      }
    )
  )

  cluster_id                         = local.fts_cluster_id
  container_port                     = var.service_configs.fts_search_indexer.port
  cpu                                = var.service_configs.fts_search_indexer.cpu
  deployment_maximum_percent         = 100
  deployment_minimum_healthy_percent = 0
  desired_count                      = var.service_configs.fts_search_indexer.desired_count
  ecs_alb_sg_id                      = "N/A"
  ecs_listener_arn                   = "N/A"
  ecs_service_base_sg_id             = var.ecs_sg_id
  extra_host_headers                 = var.fts_extra_host_headers
  family                             = "standalone"
  healthcheck_path                   = "/"
  host_port                          = var.service_configs.fts_search_indexer.port_host
  listener_name                      = "fts-${var.service_configs.fts_search_indexer.name}"
  memory                             = var.service_configs.fts_search_indexer.memory
  name                               = var.service_configs.fts_search_indexer.name
  private_subnet_ids                 = var.private_subnet_ids
  product                            = var.product
  public_domain                      = var.public_domain
  role_ecs_task_arn                  = var.role_ecs_task_arn
  role_ecs_task_exec_arn             = var.role_ecs_task_exec_arn
  tags                               = var.tags
  vpc_id                             = var.vpc_id
}
