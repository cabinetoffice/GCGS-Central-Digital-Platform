locals {
  person = {
    cpu    = 256
    memory = 512
    name   = "person"
    ports = {
      container = 8084
      host      = 8084
      listener  = 8084
    }
  }
}

module "ecs_service_person" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${local.person.name}.json.tftpl",
    {
      container_port       = local.person.ports.container
      cpu                  = local.person.cpu
      conn_string_location = var.db_connection_secret_arn
      environment          = local.service_environment
      host_port            = local.person.ports.host
      image                = "${local.ecr_urls[local.person.name]}:latest"
      lg_name              = aws_cloudwatch_log_group.tasks[local.person.name].name
      lg_prefix            = "app"
      lg_region            = data.aws_region.current.name
      memory               = local.person.memory
      name                 = local.person.name
    }
  )

  cluster_id             = aws_ecs_cluster.this.id
  container_port         = local.person.ports.container
  cpu                    = local.person.cpu
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = aws_lb_listener.ecs.arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "app"
  host_port              = local.person.ports.host
  listening_port         = local.person.ports.listener
  memory                 = local.person.memory
  name                   = local.person.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
