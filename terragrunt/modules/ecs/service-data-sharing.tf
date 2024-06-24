module "ecs_service_data_sharing" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.data_sharing.name}.json.tftpl",
    {
      container_port          = var.service_configs.data_sharing.port
      cpu                     = var.service_configs.data_sharing.cpu
      environment             = local.service_environment
      host_port               = var.service_configs.data_sharing.port
      image                   = "${local.ecr_urls[var.service_configs.data_sharing.name]}:latest"
      lg_name                 = aws_cloudwatch_log_group.tasks[var.service_configs.data_sharing.name].name
      lg_prefix               = "app"
      lg_region               = data.aws_region.current.name
      memory                  = var.service_configs.data_sharing.memory
      name                    = var.service_configs.data_sharing.name
      public_hosted_zone_fqdn = var.public_hosted_zone_fqdn
      vpc_cidr                = var.vpc_cider
    }
  )

  cluster_id             = aws_ecs_cluster.this.id
  container_port         = var.service_configs.data_sharing.port
  cpu                    = var.service_configs.data_sharing.cpu
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = aws_lb_listener.ecs.arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "app"
  host_port              = var.service_configs.data_sharing.port_host
  memory                 = var.service_configs.data_sharing.memory
  name                   = var.service_configs.data_sharing.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
