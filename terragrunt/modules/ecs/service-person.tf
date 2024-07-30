module "ecs_service_person" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.person.name}.json.tftpl",
    {
      container_port          = var.service_configs.person.port
      cpu                     = var.service_configs.person.cpu
      conn_string_location    = var.db_connection_secret_arn
      environment             = var.environment
      host_port               = var.service_configs.person.port
      image                   = "${local.ecr_urls[var.service_configs.person.name]}:${local.orchestrator_service_version}"
      lg_name                 = aws_cloudwatch_log_group.tasks[var.service_configs.person.name].name
      lg_prefix               = "app"
      lg_region               = data.aws_region.current.name
      memory                  = var.service_configs.person.memory
      name                    = var.service_configs.person.name
      public_hosted_zone_fqdn = var.public_hosted_zone_fqdn
      vpc_cidr                = var.vpc_cider
    }
  )

  cluster_id             = aws_ecs_cluster.this.id
  container_port         = var.service_configs.person.port
  cpu                    = var.service_configs.person.cpu
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = aws_lb_listener.ecs.arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "app"
  host_port              = var.service_configs.person.port
  memory                 = var.service_configs.person.memory
  name                   = var.service_configs.person.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
