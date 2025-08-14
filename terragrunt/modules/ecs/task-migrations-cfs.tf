module "ecs_migration_tasks_cfs" {
  for_each = local.migration_configs_cfs

  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${each.value.name}.json.tftpl",
    merge(
      local.cfs_migrations_container_parameters,
      {
        aws_buckets_notices = module.s3_bucket_fts.bucket
      }
    )
  )

  cluster_id             = aws_ecs_cluster.this.id
  container_port         = each.value.port
  cpu                    = each.value.cpu
  ecs_alb_sg_id          = "N/A"
  ecs_listener_arn       = "N/A"
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "db"
  is_standalone_task     = true
  memory                 = each.value.memory
  name                   = each.value.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
