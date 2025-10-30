module "ecs_service_outbox_processor_entity_verification" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.outbox_processor_entity_verification.name}.json.tftpl",
    {
      aspcore_environment    = local.aspcore_environment
      container_port         = var.service_configs.outbox_processor_entity_verification.port
      cpu                    = var.service_configs.outbox_processor_entity_verification.cpu
      db_address             = var.db_ev_cluster_address
      db_name                = var.db_ev_cluster_name
      db_password            = local.db_ev_password
      db_username            = local.db_ev_username
      host_port              = var.service_configs.outbox_processor_entity_verification.port
      image                  = local.ecr_urls[var.service_configs.outbox_processor_entity_verification.name]
      lg_name                = aws_cloudwatch_log_group.tasks[var.service_configs.outbox_processor_entity_verification.name].name
      lg_prefix              = "app"
      lg_region              = data.aws_region.current.name
      memory                 = var.service_configs.outbox_processor_entity_verification.memory
      name                   = var.service_configs.outbox_processor_entity_verification.name
      queue_organisation_url = var.queue_organisation_url
      service_version        = local.service_version_sirsi
      vpc_cidr               = var.vpc_cider
    }
  )

  cluster_id             = local.main_cluster_id
  container_port         = var.service_configs.outbox_processor_entity_verification.port
  cpu                    = var.service_configs.outbox_processor_entity_verification.cpu
  desired_count          = var.service_configs.outbox_processor_entity_verification.desired_count
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = local.main_ecs_listener_arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "app"
  host_port              = var.service_configs.outbox_processor_entity_verification.port_host
  listener_name          = "outbox-processor-ev"
  memory                 = var.service_configs.outbox_processor_entity_verification.memory
  name                   = var.service_configs.outbox_processor_entity_verification.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
