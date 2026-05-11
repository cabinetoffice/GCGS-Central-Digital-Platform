module "ecs_migration_task_fts_postgres" {

  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.fts_findtender_migrations.name}.json.tftpl",
    local.fts_findtender_migrations_container_parameters
  )

  alb_enabled            = false
  cluster_id             = local.fts_cluster_id
  cpu                    = var.service_configs.fts_findtender_migrations.cpu
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "db"
  is_standalone_task     = true
  memory                 = var.service_configs.fts_findtender_migrations.memory
  name                   = var.service_configs.fts_findtender_migrations.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
