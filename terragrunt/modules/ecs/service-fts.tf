module "ecs_service_fts" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.fts.name}.json.tftpl",
    local.fts_container_parameters
  )

  cluster_id             = local.php_cluster_id
  container_port         = var.service_configs.fts.port
  cpu                    = var.service_configs.fts.cpu
  desired_count          = var.is_production ? var.service_configs.fts.desired_count * 2 : var.service_configs.fts.desired_count // @TODO (ABN) Burn me
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = local.php_ecs_listener_arn
  ecs_service_base_sg_id = var.ecs_sg_id
  extra_host_headers     = var.fts_extra_host_headers
  family                 = "app"
  healthcheck_path       = "/health"
  host_port              = var.service_configs.fts.port
  listener_name          = local.is_php_migrated_env ?  "php-${var.service_configs.fts.name}" : null
  memory                 = var.is_production ? var.service_configs.fts.memory * 2 : var.service_configs.fts.memory // @TODO (ABN) Burn me
  name                   = var.service_configs.fts.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  user_pool_arn          = contains(["staging", "integration"], var.environment) ? var.user_pool_fts_arn : null
  user_pool_client_id    = contains(["staging", "integration"], var.environment) ? var.user_pool_fts_client_id : null
  user_pool_domain       = contains(["staging", "integration"], var.environment) ? var.user_pool_fts_domain : null
  vpc_id                 = var.vpc_id
}
