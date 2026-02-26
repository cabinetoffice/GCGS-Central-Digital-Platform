module "ecs_service_cfs" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.cfs.name}.json.tftpl",
    local.cfs_container_parameters
  )

  cluster_id             = local.php_cluster_id
  cpu                    = var.service_configs.cfs.cpu
  desired_count          = var.service_configs.cfs.desired_count
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = local.php_ecs_listener_arn
  ecs_service_base_sg_id = var.ecs_sg_id
  extra_host_headers     = var.cfs_extra_host_headers
  family                 = "app"
  healthcheck_path       = "/health"
  listener_name          = "php-${var.service_configs.cfs.name}"
  listener_priority      = var.service_configs.cfs.listener_priority
  memory                 = var.service_configs.cfs.memory
  name                   = var.service_configs.cfs.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  service_port           = local.service_port_by_cluster[var.service_configs.cfs.cluster]
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
