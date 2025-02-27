locals {
  name_prefix = var.product.resource_name

  ecs_service_migrations = ["organisation-information-migrations", "entity-verification-migrations"]
  ecs_service_names = {
    for name, config in var.service_configs :
    name => config.name if !contains(local.ecs_service_migrations, config.name)
  }

  ecs_threshold_cup_percent = 10
}
