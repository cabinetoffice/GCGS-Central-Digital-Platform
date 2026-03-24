module "ecs_migration_tasks" {
  for_each = local.migration_configs_sirsi

  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${each.value.name}.json.tftpl",
    {
      internal_service_urls     = local.internal_service_urls
      public_service_urls       = local.public_service_urls
      use_internal_service_urls = local.use_internal_service_urls
      use_internal_issuer       = local.use_internal_issuer
      aspcore_environment       = local.aspcore_environment
      cpu                       = var.service_configs.entity_verification_migrations.cpu
      db_address                = each.value.name == "entity-verification-migrations" ? var.db_ev_cluster_address : var.db_sirsi_cluster_address
      db_name                   = each.value.name == "entity-verification-migrations" ? var.db_ev_cluster_name : var.db_sirsi_cluster_name
      db_password               = each.value.name == "entity-verification-migrations" ? local.db_ev_password : local.db_sirsi_password
      db_username               = each.value.name == "entity-verification-migrations" ? local.db_ev_username : local.db_sirsi_username
      user_management_servicekey_apikey_arn = each.value.name == "organisation-information-migrations" ? data.aws_secretsmanager_secret.user_management_servicekey_apikey.arn : ""
      image                     = local.ecr_urls[each.value.name]
      internal_service_urls     = local.internal_service_urls
      lg_name                   = aws_cloudwatch_log_group.tasks[each.value.name].name
      lg_prefix                 = "db"
      lg_region                 = data.aws_region.current.region
      memory                    = each.value.memory
      name                      = each.value.name
      public_domain             = var.public_domain
      service_version           = local.service_version_sirsi
    }
  )

  alb_enabled            = false
  cluster_id             = local.main_cluster_id
  cpu                    = each.value.cpu
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
