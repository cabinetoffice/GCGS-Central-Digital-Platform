module "ecs_service_person" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.person.name}.json.tftpl",
    {
      aspcore_environment = local.aspcore_environment
      container_port      = var.service_configs.person.port
      cpu                 = var.service_configs.person.cpu
      host_port           = var.service_configs.person.port
      image               = local.ecr_urls[var.service_configs.person.name]
      lg_name             = aws_cloudwatch_log_group.tasks[var.service_configs.person.name].name
      lg_prefix           = "app"
      lg_region           = data.aws_region.current.name
      memory              = var.service_configs.person.memory
      name                = var.service_configs.person.name
      oi_db_address       = var.db_sirsi_address
      oi_db_name          = var.db_sirsi_name
      oi_db_password      = "${var.db_sirsi_credentials_arn}:username::"
      oi_db_username      = "${var.db_sirsi_credentials_arn}:password::"
      public_domain       = var.public_domain
      service_version     = local.service_version
      vpc_cidr            = var.vpc_cider
    }
  )

  cluster_id             = aws_ecs_cluster.this.id
  container_port         = var.service_configs.person.port
  cpu                    = var.service_configs.person.cpu
  desired_count          = var.service_configs.person.desired_count
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = aws_lb_listener.ecs.arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "app"
  host_port              = var.service_configs.person.port
  memory                 = var.service_configs.person.memory
  name                   = var.service_configs.person.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
