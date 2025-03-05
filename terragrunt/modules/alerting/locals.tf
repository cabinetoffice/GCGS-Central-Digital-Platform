locals {
  name_prefix = var.product.resource_name

  ecs_service_migrations = ["organisation-information-migrations", "entity-verification-migrations"]
  ecs_service_names = {
    for name, config in var.service_configs :
    name => config.name if !contains(local.ecs_service_migrations, config.name)
  }

  ecs_threshold_cup_percent    = 80
  ecs_threshold_memory_percent = 80
  ecs_threshold_4xx            = 8
  ecs_threshold_5xx            = 0

  rds_threshold_cup_percent         = 80
  rds_threshold_freeable_storage_gb = 2.4
  rds_threshold_connections         = 9000

  redis_threshold_cpu_percent             = 80
  redis_threshold_database_memory_percent = 80
  redis_threshold_evictions               = 1000

  sqs_threshold_message_age_min = 5
  sqs_threshold_queue_depth     = 1000
}
