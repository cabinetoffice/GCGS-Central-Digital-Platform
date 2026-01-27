module "ecs_service_fts_search_indexer" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.fts_search_indexer.name}.json.tftpl",
    {
      aspcore_environment = local.aspcore_environment
      container_port      = var.service_configs.fts_search_indexer.port
      cpu                 = var.service_configs.fts_search_indexer.cpu
      db_address          = var.db_fts_cluster_address
      db_name             = var.db_fts_cluster_name
      db_password         = local.db_fts_password
      db_username         = local.db_fts_username
      host_port           = var.service_configs.fts_search_indexer.port
      image               = local.ecr_urls[var.service_configs.fts_search_indexer.name]
      lg_name             = aws_cloudwatch_log_group.tasks[var.service_configs.fts_search_indexer.name].name
      lg_prefix           = "app"
      lg_region           = data.aws_region.current.region
      memory              = var.service_configs.fts_search_indexer.memory
      name                = var.service_configs.fts_search_indexer.name
      opensearch_endpoint = var.opensearch_endpoint
      service_version     = local.service_version_fts
    }
  )

  cluster_id             = local.php_cluster_id
  container_port         = var.service_configs.fts_search_indexer.port
  cpu                    = var.service_configs.fts_search_indexer.cpu
  desired_count          = var.environment == "development" ? 1 : 0
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = local.php_ecs_listener_arn
  ecs_service_base_sg_id = var.ecs_sg_id
  extra_host_headers     = var.fts_extra_host_headers
  family                 = "app"
  host_port              = var.service_configs.fts_search_indexer.port
  listener_name          = "dotnet-${var.service_configs.fts_search_indexer.name}"
  memory                 = var.service_configs.fts_search_indexer.memory
  name                   = var.service_configs.fts_search_indexer.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
