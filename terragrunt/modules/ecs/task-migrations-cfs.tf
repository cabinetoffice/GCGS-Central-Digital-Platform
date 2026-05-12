module "ecs_migration_task_cfs" {

  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.cfs_migrations.name}.json.tftpl",
    merge(
      local.cfs_migrations_container_parameters,
      {
        aws_buckets_notices = module.s3_bucket_cfs.bucket
      }
    )
  )

  alb_enabled            = false
  cluster_id             = local.php_cluster_id
  cpu                    = var.service_configs.cfs_migrations.cpu
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "db"
  is_standalone_task     = true
  memory                 = var.service_configs.cfs_migrations.memory
  name                   = var.service_configs.cfs_migrations.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
