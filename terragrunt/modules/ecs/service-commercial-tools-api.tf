module "ecs_service_commercial_tools_api" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.commercial_tools_api.name}.json.tftpl",
    {
      aspcore_environment            = local.aspcore_environment
      container_port                 = var.service_configs.commercial_tools_api.port
      cpu                            = var.service_configs.commercial_tools_api.cpu
      db_address                     = var.db_sirsi_cluster_address
      db_name                        = var.db_sirsi_cluster_name
      db_password                    = local.db_sirsi_password
      db_username                    = local.db_sirsi_username
      host_port                      = var.service_configs.commercial_tools_api.port
      image                          = local.ecr_urls[var.service_configs.commercial_tools_api.name]
      lg_name                        = aws_cloudwatch_log_group.tasks[var.service_configs.commercial_tools_api.name].name
      lg_prefix                      = "app"
      lg_region                      = data.aws_region.current.name
      memory                         = var.service_configs.commercial_tools_api.memory
      name                           = var.service_configs.commercial_tools_api.name
      odataapi_apikey                = "${data.aws_secretsmanager_secret.odi_data_platform.arn}:ApiKey::"
      public_domain                  = var.public_domain
      redis_auth_token_arn           = var.redis_auth_token_arn
      redis_port                     = var.redis_port
      redis_primary_endpoint_address = var.redis_primary_endpoint
      service_version                = var.environment == "development" ? local.service_version_sirsi : "1.0.80-bc425cdfd"
      vpc_cidr                       = var.vpc_cider
    }
  )
  cluster_id             = local.main_cluster_id
  container_port         = var.service_configs.commercial_tools_api.port
  cpu                    = var.service_configs.commercial_tools_api.cpu
  desired_count          = contains(["development", "staging"], var.environment) ? var.service_configs.commercial_tools_app.desired_count : 0
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = local.main_ecs_listener_arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "app"
  host_port              = var.service_configs.commercial_tools_api.port_host
  memory                 = var.service_configs.commercial_tools_api.memory
  name                   = var.service_configs.commercial_tools_api.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
