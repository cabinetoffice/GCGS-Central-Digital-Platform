module "ecs_task_e2e_nightly_dev" {
  source = "../ecs-service"

  count = var.service_configs.e2e_nightly_dev.desired_count == 0 ? 0 : 1

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.e2e_nightly_dev.name}.json.tftpl",
    {
      cpu                        = var.service_configs.e2e_nightly_dev.cpu
      cloudwatch_metrics_enabled = "true"
      e2e_env_secret_arn         = data.aws_secretsmanager_secret.e2e_nightly_dev_env[0].arn
      fts_secrets_arn            = data.aws_secretsmanager_secret.fts_secrets.arn
      image                      = local.ecr_urls[var.service_configs.e2e_nightly_dev.name]
      kill_after_seconds         = "30"
      lg_name                    = aws_cloudwatch_log_group.tasks[var.service_configs.e2e_nightly_dev.name].name
      lg_prefix                  = "app"
      lg_region                  = data.aws_region.current.region
      memory                     = var.service_configs.e2e_nightly_dev.memory
      name                       = var.service_configs.e2e_nightly_dev.name
      public_domain              = var.public_domain
      run_once                   = "true"
      service_version            = local.service_version_fts
      test_settings_headed       = "0"
      test_timeout_seconds       = "5400"
      teams_notification_enabled = "true"
    }
  )

  alb_enabled            = false
  cluster_id             = local.php_cluster_id
  cpu                    = var.service_configs.e2e_nightly_dev.cpu
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "standalone"
  is_standalone_task     = true
  memory                 = var.service_configs.e2e_nightly_dev.memory
  name                   = var.service_configs.e2e_nightly_dev.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
