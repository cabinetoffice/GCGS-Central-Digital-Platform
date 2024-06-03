locals {
  organisation_app = {
    cpu    = 256
    memory = 512
    name   = "organisation-app"
    ports = {
      container = 8090
      host      = 8090
      listener  = 80
    }
  }
}

module "ecs_service_organisation_app" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${local.organisation_app.name}.json.tftpl",
    {
      container_port       = local.organisation_app.ports.container
      cpu                  = local.organisation_app.cpu
      conn_string_location = var.db_connection_secret_arn
      environment          = title(var.environment)
      host_port            = local.organisation_app.ports.host
      image                = "${local.ecr_urls[local.organisation_app.name]}:latest"
      lg_name              = aws_cloudwatch_log_group.tasks[local.organisation_app.name].name
      lg_prefix            = "app"
      lg_region            = data.aws_region.current.name
      memory               = local.organisation_app.memory
      name                 = local.organisation_app.name
      oneLogin_authority   = local.one_loging.credential_locations.authority
      oneLogin_client_id   = local.one_loging.credential_locations.client_id
      oneLogin_private_key = local.one_loging.credential_locations.private_key
    }
  )

  cluster_id             = aws_ecs_cluster.this.id
  container_port         = local.organisation_app.ports.container
  cpu                    = local.organisation_app.cpu
  ecs_listener_arn       = aws_lb_listener.ecs_http.arn
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_service_base_sg_id = var.ecs_sg_id
  environment            = var.environment
  family                 = "app"
  host_port              = local.organisation_app.ports.host
  listening_port         = local.organisation_app.ports.listener
  memory                 = local.organisation_app.memory
  name                   = local.organisation_app.name
  private_subnet_ids     = var.private_subnet_ids
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
