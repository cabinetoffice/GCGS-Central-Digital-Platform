module "ecs_service_commercial_tools_app" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.commercial_tools_app.name}.json.tftpl",
    {
      aspcore_environment               = local.aspcore_environment
      cpu                               = var.service_configs.commercial_tools_app.cpu
      diagnostic_page_enabled           = !var.is_production || var.environment == "integration"
      diagnostic_page_path_arn          = aws_secretsmanager_secret.cdp_sirsi_diagnostic_path.arn
      image                             = local.ecr_urls[var.service_configs.commercial_tools_app.name]
      internal_service_urls             = local.internal_service_urls
      lg_name                           = aws_cloudwatch_log_group.tasks[var.service_configs.commercial_tools_app.name].name
      lg_prefix                         = "app"
      lg_region                         = data.aws_region.current.region
      memory                            = var.service_configs.commercial_tools_app.memory
      name                              = var.service_configs.commercial_tools_app.name
      onelogin_account_url              = local.one_login.credential_locations.account_url
      onelogin_authority                = local.one_login.credential_locations.authority
      onelogin_client_id                = local.one_login.credential_locations.client_id
      onelogin_fln_api_key_arn          = data.aws_secretsmanager_secret.one_login_forward_logout_notification_api_key.arn
      onelogin_logout_notification_urls = local.onelogin_logout_notification_urls
      onelogin_private_key              = local.one_login.credential_locations.private_key
      public_domain                     = var.public_domain
      public_service_urls               = local.public_service_urls
      redis_auth_token_arn              = var.redis_auth_token_arn
      redis_port                        = var.redis_port
      redis_primary_endpoint_address    = var.redis_primary_endpoint
      service_port                      = local.service_ports_by_service[var.service_configs.commercial_tools_app.name]
      service_version                   = var.environment == "development" ? local.service_version_sirsi : "1.0.80-98036a04a"
      sessiontimeoutinminutes           = var.commercial_tools_session_timeout
      shared_sessions_enabled           = local.shared_sessions_enabled
      ssm_data_protection_prefix        = local.ssm_data_protection_prefix
      use_internal_issuer               = local.use_internal_issuer
      use_internal_service_urls         = local.use_internal_service_urls
      vpc_cidr                          = var.vpc_cider
    }
  )

  allowed_unauthenticated_paths = local.unauthenticated_assets_paths
  cluster_id                    = local.main_cluster_id
  cpu                           = var.service_configs.commercial_tools_app.cpu
  desired_count                 = contains(["development", "staging"], var.environment) ? var.service_configs.commercial_tools_api.desired_count : 0
  ecs_alb_sg_id                 = var.alb_sg_id
  ecs_listener_arn              = local.main_ecs_listener_arn
  ecs_service_base_sg_id        = var.ecs_sg_id
  family                        = "app"
  internal_alb_enabled          = local.use_internal_service_urls
  internal_domain               = local.internal_domain
  internal_listener_arn         = local.internal_ecs_listener_arn
  is_frontend_app               = false
  listener_priority             = var.service_configs.commercial_tools_app.listener_priority
  path_routing_rules = [
    {
      host_headers  = [var.public_domain]
      path_patterns = ["/search/commercial-tools", "/commercial-tools/*"]
      priority      = var.service_configs.commercial_tools_app.listener_priority - 5
    }
  ]
  additional_external_target_groups = []
  memory                        = var.service_configs.commercial_tools_app.memory
  name                          = var.service_configs.commercial_tools_app.name
  private_subnet_ids            = var.private_subnet_ids
  product                       = var.product
  public_domain                 = var.public_domain
  role_ecs_task_arn             = var.role_ecs_task_arn
  role_ecs_task_exec_arn        = var.role_ecs_task_exec_arn
  service_port                  = local.service_ports_by_service[var.service_configs.commercial_tools_app.name]
  tags                          = var.tags
  user_pool_arn                 = local.cognito_enabled ? var.user_pool_arn : null
  user_pool_client_id           = local.cognito_enabled ? var.user_pool_commercial_tools_client_id : null
  user_pool_domain              = local.cognito_enabled ? var.user_pool_domain : null
  vpc_id                        = var.vpc_id
}
