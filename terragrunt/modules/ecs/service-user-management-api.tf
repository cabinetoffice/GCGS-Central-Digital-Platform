module "ecs_service_user_management_api" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.user_management_api.name}.json.tftpl",
    {
      aspcore_environment               = local.aspcore_environment
      authentication_authority          = data.aws_secretsmanager_secret.user_management_authentication_authority.arn
      container_port                    = var.service_configs.user_management_api.port
      cpu                               = var.service_configs.user_management_api.cpu
      db_address                        = var.db_sirsi_cluster_address
      db_name                           = var.db_sirsi_cluster_name
      db_password                       = local.db_sirsi_password
      db_username                       = local.db_sirsi_username
      host_port                         = var.service_configs.user_management_api.port
      image                             = local.ecr_urls[var.service_configs.user_management_api.name]
      lg_name                           = aws_cloudwatch_log_group.tasks[var.service_configs.user_management_api.name].name
      lg_prefix                         = "app"
      lg_region                         = data.aws_region.current.region
      memory                            = var.service_configs.user_management_api.memory
      name                              = var.service_configs.user_management_api.name
      public_domain                     = var.public_domain
      service_version                   = local.service_version_sirsi
      servicekey_apikey                 = data.aws_secretsmanager_secret.user_management_servicekey_apikey.arn
      vpc_cidr                          = var.vpc_cider
    }
  )

  cluster_id             = local.main_cluster_id
  container_port         = var.service_configs.user_management_api.port
  cpu                    = var.service_configs.user_management_api.cpu
  desired_count          = var.service_configs.user_management_api.desired_count
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = local.main_ecs_listener_arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "app"
  host_port              = var.service_configs.user_management_api.port_host
  memory                 = var.service_configs.user_management_api.memory
  name                   = var.service_configs.user_management_api.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
