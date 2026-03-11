# module "ecs_service_user_management_app" {
#   source = "../ecs-service"
#
#   container_definitions = templatefile(
#     "${path.module}/templates/task-definitions/${var.service_configs.user_management_app.name}.json.tftpl",
#     {
#       aspcore_environment            = local.aspcore_environment
#       container_port                 = local.service_ports_by_service[var.service_configs.user_management_app.name]
#       cpu                            = var.service_configs.user_management_app.cpu
#       host_port                      = local.service_ports_by_service[var.service_configs.user_management_app.name]
#       image                          = local.ecr_urls[var.service_configs.fts_healthcheck.name] # local.ecr_urls[var.service_configs.user_management_app.name]
#       internal_service_urls          = local.internal_service_urls
#       lg_name                        = aws_cloudwatch_log_group.tasks[var.service_configs.user_management_app.name].name
#       lg_prefix                      = "app"
#       lg_region                      = data.aws_region.current.region
#       memory                         = var.service_configs.user_management_app.memory
#       name                           = var.service_configs.user_management_app.name
#       onelogin_account_url           = local.one_login.credential_locations.account_url
#       onelogin_authority             = local.one_login.credential_locations.authority
#       onelogin_client_id             = local.one_login.credential_locations.client_id
#       onelogin_private_key           = local.one_login.credential_locations.private_key
#       public_domain                  = var.public_domain
#       public_service_urls            = local.public_service_urls
#       redis_auth_token_arn           = var.redis_auth_token_arn
#       redis_port                     = var.redis_port
#       redis_primary_endpoint_address = var.redis_primary_endpoint
#       service_version                = local.service_version_sirsi
#       ssm_data_protection_prefix     = local.ssm_data_protection_prefix
#       use_internal_service_urls      = local.use_internal_service_urls
#       vpc_cidr                       = var.vpc_cider
#     }
#   )
#
#   allowed_unauthenticated_paths = local.unauthenticated_assets_paths
#   cluster_id                    = local.main_cluster_id
#   cpu                           = var.service_configs.user_management_app.cpu
#   desired_count                 = var.service_configs.user_management_app.desired_count
#   ecs_alb_sg_id                 = var.alb_sg_id
#   ecs_listener_arn              = local.main_ecs_listener_arn
#   ecs_service_base_sg_id        = var.ecs_sg_id
#   family                        = "app"
#   is_frontend_app               = true
#   listener_priority             = var.service_configs.user_management_app.listener_priority
#   memory                        = var.service_configs.user_management_app.memory
#   name                          = var.service_configs.user_management_app.name
#   private_subnet_ids            = var.private_subnet_ids
#   product                       = var.product
#   public_domain                 = var.public_domain
#   role_ecs_task_arn             = var.role_ecs_task_arn
#   role_ecs_task_exec_arn        = var.role_ecs_task_exec_arn
#   service_port                  = local.service_ports_by_service[var.service_configs.user_management_app.name]
#   tags                          = var.tags
#   user_pool_arn                 = local.cognito_enabled ? var.user_pool_arn : null
#   user_pool_client_id           = local.cognito_enabled ? var.user_pool_client_id : null
#   user_pool_domain              = local.cognito_enabled ? var.user_pool_domain : null
#   vpc_id                        = var.vpc_id
# }
