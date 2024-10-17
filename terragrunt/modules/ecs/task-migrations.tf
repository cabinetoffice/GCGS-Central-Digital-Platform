module "ecs_migration_tasks" {
  for_each = local.migration_configs

  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${each.value.name}.json.tftpl",
    {
      cpu                 = var.service_configs.entity_verification_migrations.cpu
      aspcore_environment = local.aspcore_environment
      image               = local.ecr_urls[each.value.name]
      lg_name             = aws_cloudwatch_log_group.tasks[each.value.name].name
      lg_prefix           = "db"
      lg_region           = data.aws_region.current.name
      memory              = each.value.memory
      name                = each.value.name
      db_address          = each.value.name == "entity-verification-migrations" ? var.db_entity_verification_address : var.db_sirsi_address
      db_name             = each.value.name == "entity-verification-migrations" ? var.db_entity_verification_name : var.db_sirsi_name
      db_password         = each.value.name == "entity-verification-migrations" ? "${var.db_entity_verification_credentials_arn}:username::" : "${var.db_sirsi_credentials_arn}:username::"
      db_username         = each.value.name == "entity-verification-migrations" ? "${var.db_entity_verification_credentials_arn}:password::" : "${var.db_sirsi_credentials_arn}:password::"
      public_domain       = var.public_domain
      service_version     = local.service_version
    }
  )

  cluster_id             = aws_ecs_cluster.this.id
  container_port         = each.value.port
  cpu                    = each.value.cpu
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = aws_lb_listener.ecs.arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "db"
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
