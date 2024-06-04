locals {
  organisation = {
    cpu    = 256
    memory = 512
    name   = "organisation"
    ports = {
      container = 8082
      host      = 8082
      listener  = 8082
    }
  }
}

module "ecs_service_organisation" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${local.organisation.name}.json.tftpl",
    {
      container_port       = local.organisation.ports.container
      cpu                  = local.organisation.cpu
      conn_string_location = var.db_connection_secret_arn
      environment          = title(var.environment)
      host_port            = local.organisation.ports.host
      image                = "${local.ecr_urls[local.organisation.name]}:latest"
      lg_name              = aws_cloudwatch_log_group.tasks[local.organisation.name].name
      lg_prefix            = "app"
      lg_region            = data.aws_region.current.name
      memory               = local.organisation.memory
      name                 = local.organisation.name
    }
  )

  cluster_id             = aws_ecs_cluster.this.id
  container_port         = local.organisation.ports.container
  cpu                    = local.organisation.cpu
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = aws_lb_listener.ecs.arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "app"
  host_port              = local.organisation.ports.host
  listening_port         = local.organisation.ports.listener
  memory                 = local.organisation.memory
  name                   = local.organisation.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
