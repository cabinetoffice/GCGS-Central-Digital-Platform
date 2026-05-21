module "ecs_service_fts_notice_publish_worker" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.fts_notice_publish_worker.name}.json.tftpl",
    local.fts_notice_publish_worker_container_parameters
  )

  alb_enabled            = false
  cluster_id             = local.php_cluster_id
  cpu                    = var.service_configs.fts_notice_publish_worker.cpu
  desired_count          = var.is_production ? 0 : var.service_configs.fts_notice_publish_worker.desired_count
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "standalone"
  memory                 = local.is_prod_or_staging ? var.service_configs.fts_notice_publish_worker.memory * 2 : var.service_configs.fts_notice_publish_worker.memory
  name                   = var.service_configs.fts_notice_publish_worker.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  role_ecs_task_arn       = var.role_ecs_task_arn
  role_ecs_task_exec_arn  = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
