locals {
  forms = {
    cpu    = 256
    memory = 512
    name   = "forms"
    ports = {
      container = 8086
      host      = 8086
      listener  = 8086
    }
  }
}

module "ecs_service_forms" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${local.forms.name}.json.tftpl",
    {
      container_port = local.forms.ports.container
      cpu            = local.forms.cpu
      environment    = title(var.environment)
      host_port      = local.forms.ports.host
      image          = "${local.ecr_urls[local.forms.name]}:latest"
      lg_name        = aws_cloudwatch_log_group.tasks[local.forms.name].name
      lg_prefix      = "app"
      lg_region      = data.aws_region.current.name
      memory         = local.forms.memory
      name           = local.forms.name
    }
  )

  cluster_id             = aws_ecs_cluster.this.id
  container_port         = local.forms.ports.container
  cpu                    = local.forms.cpu
  ecs_alb_arn            = aws_lb.ecs.arn
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_service_base_sg_id = var.ecs_sg_id
  environment            = var.environment
  family                 = "app"
  listening_port         = local.forms.ports.listener
  memory                 = local.forms.memory
  name                   = local.forms.name
  private_subnet_ids     = var.private_subnet_ids
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
  host_port              = local.forms.ports.host
}
