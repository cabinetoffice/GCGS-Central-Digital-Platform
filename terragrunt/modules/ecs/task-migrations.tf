module "ecs_migration_tasks" {
  for_each = local.migration_configs

  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${each.value.name}.json.tftpl",
    {
      cpu                     = var.service_configs.entity_verification_migrations.cpu
      conn_string_location    = var.db_connection_secret_arn
      environment             = local.service_environment
      image                   = "${local.ecr_urls[each.value.name]}:${local.orchestrator_service_version}"
      lg_name                 = aws_cloudwatch_log_group.tasks[each.value.name].name
      lg_prefix               = "db"
      lg_region               = data.aws_region.current.name
      memory                  = each.value.memory
      name                    = each.value.name
      public_hosted_zone_fqdn = var.public_hosted_zone_fqdn
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
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
