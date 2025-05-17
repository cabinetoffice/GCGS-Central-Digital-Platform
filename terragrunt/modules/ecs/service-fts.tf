module "ecs_service_fts" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.fts.name}.json.tftpl",
    {
      container_port  = var.service_configs.fts.port
      cpu             = var.service_configs.fts.cpu
      db_host         = var.db_fts_cluster_address
      db_name         = var.db_fts_cluster_name
      db_pass         = local.db_fts_password
      db_user         = local.db_fts_username
      host_port       = var.service_configs.fts.port
      image           = local.ecr_urls[var.service_configs.fts.name]
      lg_name         = aws_cloudwatch_log_group.tasks[var.service_configs.fts.name].name
      lg_prefix       = "app"
      lg_region       = data.aws_region.current.name
      memory          = var.service_configs.fts.memory
      name            = var.service_configs.fts.name
      public_domain   = var.public_domain
      service_version = "latest" //local.service_version
      vpc_cidr        = var.vpc_cider
    }
  )

  cluster_id             = aws_ecs_cluster.this.id
  container_port         = var.service_configs.fts.port
  cpu                    = var.service_configs.fts.cpu
  desired_count          = var.service_configs.fts.desired_count
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = aws_lb_listener.ecs.arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "app"
  healthcheck_path       = "/healthz.php"
  host_port              = var.service_configs.fts.port
  memory                 = var.service_configs.fts.memory
  name                   = var.service_configs.fts.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
