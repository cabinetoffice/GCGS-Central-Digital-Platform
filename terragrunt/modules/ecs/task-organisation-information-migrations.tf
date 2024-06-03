locals {
  organisation_information_migrations = {
    cpu    = 256
    memory = 512
    ports = {
      container = 9090
      host      = 9090
    }
    name = "organisation-information-migrations"
  }
}

module "ecs_service_organisation_information_migrations" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${local.organisation_information_migrations.name}.json.tftpl",
    {
      container_port       = local.organisation_information_migrations.ports.container
      cpu                  = local.organisation_information_migrations.cpu
      conn_string_location = var.db_connection_secret_arn
      environment          = title(var.environment)
      host_port            = local.organisation_information_migrations.ports.host
      image                = "${local.ecr_urls["organisation-information-migrations"]}:latest"
      lg_name              = aws_cloudwatch_log_group.tasks[local.organisation_information_migrations.name].name
      lg_prefix            = "db"
      lg_region            = data.aws_region.current.name
      memory               = local.organisation_information_migrations.memory
      name                 = local.organisation_information_migrations.name
    }
  )

  cluster_id             = aws_ecs_cluster.this.id
  container_port         = local.organisation_information_migrations.ports.container
  cpu                    = local.organisation_information_migrations.cpu
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = aws_lb_listener.ecs.arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "db"
  host_port              = local.organisation_information_migrations.ports.host
  memory                 = local.organisation_information_migrations.memory
  name                   = local.organisation_information_migrations.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
