locals {
  data_sharing = {
    cpu    = 256
    memory = 512
    name   = "data-sharing"
    ports = {
      container = 8088
      host      = 8088
      listener  = 8088
    }
  }
}

module "ecs_service_data_sharing" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${local.data_sharing.name}.json.tftpl",
    {
      container_port       = local.data_sharing.ports.container
      cpu                  = local.data_sharing.cpu
      conn_string_location = var.db_connection_secret_arn
      environment          = title(var.environment)
      host_port            = local.data_sharing.ports.host
      image                = "${local.ecr_urls[local.data_sharing.name]}:latest"
      lg_name              = aws_cloudwatch_log_group.tasks[local.data_sharing.name].name
      lg_prefix            = "app"
      lg_region            = data.aws_region.current.name
      memory               = local.data_sharing.memory
      name                 = local.data_sharing.name
    }
  )

  cluster_id             = aws_ecs_cluster.this.id
  container_port         = local.data_sharing.ports.container
  cpu                    = local.data_sharing.cpu
  ecs_listener_arn       = aws_lb_listener.ecs_http.arn
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_service_base_sg_id = var.ecs_sg_id
  environment            = var.environment
  family                 = "app"
  host_port              = local.data_sharing.ports.host
  listening_port         = local.data_sharing.ports.listener
  memory                 = local.data_sharing.memory
  name                   = local.data_sharing.name
  private_subnet_ids     = var.private_subnet_ids
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
