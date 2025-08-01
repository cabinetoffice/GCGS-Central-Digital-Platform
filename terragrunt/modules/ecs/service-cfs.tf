module "ecs_service_cfs" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.cfs.name}.json.tftpl",
    local.cfs_container_parameters
  )

  cluster_id             = aws_ecs_cluster.this.id
  container_port         = var.service_configs.cfs.port
  cpu                    = var.service_configs.cfs.cpu
  desired_count          = var.service_configs.cfs.desired_count
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = aws_lb_listener.ecs.arn
  ecs_service_base_sg_id = var.ecs_sg_id
  extra_host_headers     = var.cfs_extra_host_headers
  family                 = "app"
  healthcheck_path       = "/health"
  host_port              = var.service_configs.cfs.port
  memory                 = var.service_configs.cfs.memory
  name                   = var.service_configs.cfs.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  user_pool_arn          = contains(["staging", "integration", "production"], var.environment) ? var.user_pool_cfs_arn : null
  user_pool_client_id    = contains(["staging", "integration", "production"], var.environment) ? var.user_pool_cfs_client_id : null
  user_pool_domain       = contains(["staging", "integration", "production"], var.environment) ? var.user_pool_cfs_domain : null
  vpc_id                 = var.vpc_id
}
