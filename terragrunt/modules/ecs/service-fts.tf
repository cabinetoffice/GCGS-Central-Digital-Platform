module "ecs_service_fts" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.fts.name}.json.tftpl",
    local.fts_container_parameters
  )

  cluster_id             = aws_ecs_cluster.this.id
  container_port         = var.service_configs.fts.port
  cpu                    = var.service_configs.fts.cpu
  desired_count          = var.service_configs.fts.desired_count
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = aws_lb_listener.ecs.arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "app"
  healthcheck_path       = "/" #"/healthz.php"
  host_port              = var.service_configs.fts.port
  memory                 = var.service_configs.fts.memory
  name                   = var.service_configs.fts.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  user_pool_arn          = var.user_pool_fts_arn
  user_pool_client_id    = var.user_pool_fts_client_id
  user_pool_domain       = var.user_pool_fts_domain
  vpc_id                 = var.vpc_id
}
