module "ecs_service_av_scanner_app" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.av_scanner_app.name}.json.tftpl",
    {
      aspcore_environment      = local.aspcore_environment
      container_port           = var.service_configs.av_scanner_app.port
      cpu                      = var.service_configs.av_scanner_app.cpu
      host_port                = var.service_configs.av_scanner_app.port
      image                    = local.ecr_urls[var.service_configs.av_scanner_app.name]
      lg_name                  = aws_cloudwatch_log_group.tasks[var.service_configs.av_scanner_app.name].name
      lg_prefix                = "app"
      lg_region                = data.aws_region.current.name
      memory                   = var.service_configs.av_scanner_app.memory
      name                     = var.service_configs.av_scanner_app.name
      public_domain            = var.public_domain
      queue_av_scanner_url     = var.queue_av_scanner_url
      service_version          = local.service_version
      uuid_ppon_service_enable = false
      vpc_cidr                 = var.vpc_cider
    }
  )

  cluster_id             = aws_ecs_cluster.this.id
  container_port         = var.service_configs.av_scanner_app.port
  cpu                    = var.service_configs.av_scanner_app.cpu
  desired_count          = var.service_configs.av_scanner_app.desired_count
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = aws_lb_listener.ecs.arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "app"
  host_port              = var.service_configs.av_scanner_app.port
  memory                 = var.service_configs.av_scanner_app.memory
  name                   = var.service_configs.av_scanner_app.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
