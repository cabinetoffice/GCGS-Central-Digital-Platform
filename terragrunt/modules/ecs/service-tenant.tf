locals {
  tenant = {
    cpu    = 256
    memory = 512
    name   = "tenant"
    ports = {
      container = 8080
      host      = 8080
      listener  = 8080
    }
  }
}

module "ecs_service_tenant" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${local.tenant.name}.json.tftpl",
    {
      container_port       = local.tenant.ports.container
      cpu                  = local.tenant.cpu
      conn_string_location = var.db_connection_secret_arn
      environment          = title(var.environment)
      host_port            = local.tenant.ports.host
      image                = "${local.ecr_urls[local.tenant.name]}:latest"
      lg_name              = aws_cloudwatch_log_group.tasks[local.tenant.name].name
      lg_prefix            = "app"
      lg_region            = data.aws_region.current.name
      memory               = local.tenant.memory
      name                 = local.tenant.name
    }
  )

  cluster_id             = aws_ecs_cluster.this.id
  container_port         = local.tenant.ports.container
  cpu                    = local.tenant.cpu
  ecs_alb_arn            = aws_lb.ecs.arn
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_service_base_sg_id = var.ecs_sg_id
  environment            = var.environment
  family                 = "app"
  listening_port         = local.tenant.ports.listener
  memory                 = local.tenant.memory
  name                   = local.tenant.name
  private_subnet_ids     = var.private_subnet_ids
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
  host_port              = local.tenant.ports.host
}
