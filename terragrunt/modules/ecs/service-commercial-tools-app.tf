module "ecs_service_commercial_tools_app" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.commercial_tools_app.name}.json.tftpl",
    {
      aspcore_environment            = local.aspcore_environment
      container_port                 = var.service_configs.commercial_tools_app.port
      cpu                            = var.service_configs.commercial_tools_app.cpu
      host_port                      = var.service_configs.commercial_tools_app.port
      image                          = local.ecr_urls[var.service_configs.commercial_tools_app.name]
      lg_name                        = aws_cloudwatch_log_group.tasks[var.service_configs.organisation_app.name].name
      lg_prefix                      = "app"
      lg_region                      = data.aws_region.current.name
      memory                         = var.service_configs.commercial_tools_app.memory
      name                           = var.service_configs.commercial_tools_app.name
      public_domain                  = var.public_domain
      redis_auth_token_arn           = var.redis_auth_token_arn
      redis_port                     = var.redis_port
      redis_primary_endpoint_address = var.redis_primary_endpoint
      service_version                = local.service_version_sirsi
      sessiontimeoutinminutes        = var.commercial_tools_session_timeout
      vpc_cidr                       = var.vpc_cider
      onelogin_account_url              = local.one_login.credential_locations.account_url
      onelogin_authority                = local.one_login.credential_locations.authority
      onelogin_client_id                = local.one_login.credential_locations.client_id
      onelogin_fln_api_key_arn          = data.aws_secretsmanager_secret.one_login_forward_logout_notification_api_key.arn
      onelogin_logout_notification_urls = local.onelogin_logout_notification_urls
      onelogin_private_key              = local.one_login.credential_locations.private_key
    }
  )

  cluster_id                    = aws_ecs_cluster.this.id
  container_port                = var.service_configs.commercial_tools_app.port
  cpu                           = var.service_configs.commercial_tools_app.cpu
  desired_count                 = var.environment == "development" ? 2 : 0 // var.service_configs.commercial_tools_app.desired_count
  ecs_alb_sg_id                 = var.alb_sg_id
  ecs_listener_arn              = aws_lb_listener.ecs.arn
  ecs_service_base_sg_id        = var.ecs_sg_id
  family                        = "app"
  host_port                     = var.service_configs.commercial_tools_app.port # this needs to stay and can't be same as host, 80
  is_frontend_app               = true
  memory                        = var.service_configs.commercial_tools_app.memory
  name                          = var.service_configs.commercial_tools_app.name
  private_subnet_ids            = var.private_subnet_ids
  product                       = var.product
  public_domain                 = var.public_domain
  role_ecs_task_arn             = var.role_ecs_task_arn
  role_ecs_task_exec_arn        = var.role_ecs_task_exec_arn
  tags                          = var.tags
  allowed_unauthenticated_paths = ["/one-login/back-channel-sign-out", "/assets/*", "/css/*", "/manifest.json"]
  user_pool_arn                 = local.cognito_enabled ? var.user_pool_arn : null
  user_pool_client_id           = local.cognito_enabled ? var.user_pool_client_id : null
  user_pool_domain              = local.cognito_enabled ? var.user_pool_domain : null
  vpc_id                        = var.vpc_id
}
