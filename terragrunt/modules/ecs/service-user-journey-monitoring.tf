module "ecs_service_user_journey_monitoring" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.user_journey_monitoring.name}.json.tftpl",
    {
      betterstack_secret_arn       = data.aws_secretsmanager_secret.betterstack_user_journey_monitoring.arn
      fts_secrets_arn              = data.aws_secretsmanager_secret.fts_secrets.arn
      cpu                          = var.service_configs.user_journey_monitoring.cpu
      desired_count                = var.service_configs.user_journey_monitoring.desired_count
      image                        = local.ecr_urls[var.service_configs.user_journey_monitoring.name]
      lg_name                      = aws_cloudwatch_log_group.tasks[var.service_configs.user_journey_monitoring.name].name
      lg_prefix                    = "app"
      lg_region                    = data.aws_region.current.region
      memory                       = var.service_configs.user_journey_monitoring.memory
      name                         = var.service_configs.user_journey_monitoring.name
      service_version              = local.orchestrator_fts_service_version
      test_env                     = var.environment
      test_settings_fts_public_url = "https://${local.fts_site_domains[var.environment]}"
    }
  )

  alb_enabled            = false
  cluster_id             = local.php_cluster_id
  cpu                    = var.service_configs.user_journey_monitoring.cpu
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "standalone"
  memory                 = var.service_configs.user_journey_monitoring.memory
  name                   = var.service_configs.user_journey_monitoring.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  role_ecs_task_arn       = var.role_ecs_task_arn
  role_ecs_task_exec_arn  = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
