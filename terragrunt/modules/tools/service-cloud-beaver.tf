module "ecs_service_cloud_beaver" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.cloud_beaver_config.name}.json.tftpl",
    {
      account_id                 = data.aws_caller_identity.current.account_id
      cb_admin_password          = "${aws_secretsmanager_secret.cloud_beaver_credentials.arn}:ADMIN_PASSWORD::"
      cb_admin_username          = "${aws_secretsmanager_secret.cloud_beaver_credentials.arn}:ADMIN_USERNAME::"
      cb_data_sources_secret_arn = aws_secretsmanager_secret_version.cloud_beaver_data_sources.arn
      cb_server_name             = "CloudBeaver ${title(var.environment)} Server"
      cb_server_url              = "https://${var.cloud_beaver_config.name}.${var.public_domain}"
      container_path             = local.cloud_beaver_container_path
      container_port             = var.cloud_beaver_config.port
      cpu                        = var.cloud_beaver_config.cpu
      efs_access_point_id        = aws_efs_access_point.cloudbeaver.id
      efs_file_system_id         = aws_efs_file_system.cloudbeaver.id
      host_port                  = var.cloud_beaver_config.port
      image                      = "${local.orchestrator_account_id}.dkr.ecr.${data.aws_region.current.region}.amazonaws.com/cdp-${var.cloud_beaver_config.name}:latest"
      lg_name                    = aws_cloudwatch_log_group.cloud_beaver.name
      lg_prefix                  = "tools"
      lg_region                  = data.aws_region.current.region
      memory                     = var.cloud_beaver_config.memory
      name                       = var.cloud_beaver_config.name
      source_volume              = local.cloud_beaver_volume_name
    }
  )

  cluster_id                         = var.ecs_cluster_id
  container_port                     = var.cloud_beaver_config.port
  cpu                                = var.cloud_beaver_config.cpu
  deployment_maximum_percent         = 100
  deployment_minimum_healthy_percent = 0
  ecs_alb_sg_id                      = var.alb_tools_sg_id
  ecs_listener_arn                   = aws_lb_listener.tools.arn
  ecs_service_base_sg_id             = var.ecs_sg_id
  efs_volume = {
    access_point_id    = aws_efs_access_point.cloudbeaver.id
    container_path     = local.cloud_beaver_container_path
    file_system_id     = aws_efs_file_system.cloudbeaver.id
    iam                = "DISABLED"
    name               = local.cloud_beaver_volume_name
    transit_encryption = "ENABLED"
  }
  family                        = "tools"
  healthcheck_healthy_threshold = 3
  healthcheck_interval          = 60
  healthcheck_path              = "/status"
  healthcheck_timeout           = 40
  host_port                     = var.cloud_beaver_config.port
  memory                        = var.cloud_beaver_config.memory
  name                          = var.cloud_beaver_config.name
  private_subnet_ids            = var.private_subnet_ids
  product                       = var.product
  public_domain                 = var.public_domain
  role_ecs_task_arn             = var.role_ecs_task_arn
  role_ecs_task_exec_arn        = var.role_ecs_task_exec_arn
  tags                          = var.tags
  unhealthy_threshold           = 6
  user_pool_arn                 = var.user_pool_arn_cloud_beaver
  user_pool_client_id           = var.user_pool_client_id_cloud_beaver
  user_pool_domain              = var.user_pool_domain_cloud_beaver
  vpc_id                        = var.vpc_id
}
