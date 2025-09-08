module "ecs_service_commercial_tools_api" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.commercial_tools_api.name}.json.tftpl",
    {
      aspcore_environment             = local.aspcore_environment
      container_port                  = var.service_configs.commercial_tools_api.port
      cpu                             = var.service_configs.commercial_tools_api.cpu
      host_port                       = var.service_configs.commercial_tools_api.port
      image                           = local.ecr_urls[var.service_configs.commercial_tools_api.name]
      lg_name                         = aws_cloudwatch_log_group.tasks[var.service_configs.commercial_tools_api.name].name
      lg_prefix                       = "app"
      lg_region                       = data.aws_region.current.name
      memory                          = var.service_configs.commercial_tools_api.memory
      name                            = var.service_configs.commercial_tools_api.name
      odataapi_baseurl                = "${data.aws_secretsmanager_secret.odi_data_platform_secret.arn}:BaseUrl::"
      odataapi_apikey                 = "${data.aws_secretsmanager_secret.odi_data_platform_secret.arn}:ApiKey::"
      public_domain                   = var.public_domain
      service_version                 = local.service_version_sirsi
      vpc_cidr                        = var.vpc_cider      
    }
  )
  cluster_id             = aws_ecs_cluster.this.id
  container_port         = var.service_configs.commercial_tools_api.port
  cpu                    = var.service_configs.commercial_tools_api.cpu
  desired_count          = var.service_configs.commercial_tools_api.desired_count
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = aws_lb_listener.ecs.arn
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
  unhealthy_threshold    = 10  # @todo (ABN) go-live this must come out, hopefully before monday!
  healthcheck_interval   = 120 # @todo (ABN) go-live this must come out, hopefully before monday!
  healthcheck_timeout    = 10  # @todo (ABN) go-live this must come out, hopefully before monday!
  vpc_id                 = var.vpc_id
}
