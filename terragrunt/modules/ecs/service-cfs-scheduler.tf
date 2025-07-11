module "ecs_service_cfs_scheduler" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.cfs_scheduler.name}.json.tftpl",
    merge(
      local.cfs_scheduler_container_parameters,
      {
        aws_buckets_notices = module.s3_bucket_cfs.bucket
      }
    )
  )

  cluster_id             = aws_ecs_cluster.this.id
  container_port         = var.service_configs.cfs_scheduler.port
  cpu                    = var.service_configs.cfs_scheduler.cpu
  desired_count          = var.service_configs.cfs_scheduler.desired_count
  ecs_alb_sg_id          = "N/A"
  ecs_listener_arn       = "N/A"
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "standalone"
  healthcheck_path       = "/" #"/healthz.php"
  host_port              = var.service_configs.cfs_scheduler.port_host
  memory                 = var.service_configs.cfs_scheduler.memory
  name                   = var.service_configs.cfs_scheduler.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
