module "ecs_service_fts_pdf_render_service" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.fts_pdf_render_service.name}.json.tftpl",
    merge(
      local.fts_pdf_render_service_container_parameters,
      {
        cpu          = var.service_configs.fts_pdf_render_service.cpu
        image        = local.ecr_urls[var.service_configs.fts_pdf_render_service.name]
        lg_name      = aws_cloudwatch_log_group.tasks[var.service_configs.fts_pdf_render_service.name].name
        memory       = var.service_configs.fts_pdf_render_service.memory
        name         = var.service_configs.fts_pdf_render_service.name
        service_port = local.service_ports_by_service[var.service_configs.fts_pdf_render_service.name]
      }
    )
  )

  alb_enabled            = false
  cluster_id             = local.php_cluster_id
  cpu                    = var.service_configs.fts_pdf_render_service.cpu
  desired_count          = var.service_configs.fts_pdf_render_service.desired_count
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "app"
  internal_alb_enabled   = true
  internal_domain        = local.internal_domain
  internal_listener_arn  = local.internal_ecs_listener_arn
  listener_name          = var.service_configs.fts_pdf_render_service.name
  listener_priority      = var.service_configs.fts_pdf_render_service.listener_priority
  memory                 = var.service_configs.fts_pdf_render_service.memory
  name                   = var.service_configs.fts_pdf_render_service.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  service_port           = local.service_ports_by_service[var.service_configs.fts_pdf_render_service.name]
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
