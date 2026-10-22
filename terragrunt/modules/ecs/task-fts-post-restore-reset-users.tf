locals {
  fts_post_restore_reset_users_task_name = "fts-post-restore-reset-users"
}

resource "aws_cloudwatch_log_group" "fts_post_restore_reset_users" {
  count = var.environment == "staging" ? 1 : 0

  name              = "/ecs/${local.fts_post_restore_reset_users_task_name}"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

module "ecs_task_fts_post_restore_reset_users" {
  source = "../ecs-service"

  count = var.environment == "staging" ? 1 : 0

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/fts-post-restore-reset-users.json.tftpl",
    {
      fts_app_secrets_arn   = data.aws_secretsmanager_secret.fts_secrets.arn
      image                 = "public.ecr.aws/docker/library/mysql:8.0"
      lg_name               = aws_cloudwatch_log_group.fts_post_restore_reset_users[0].name
      lg_prefix             = "reset-users"
      lg_region             = data.aws_region.current.region
      master_secret_arn     = data.aws_secretsmanager_secret.fts_db_master_password[0].arn
      name                  = local.fts_post_restore_reset_users_task_name
      tool_users_secret_arn = data.aws_secretsmanager_secret.fts_db_tool_user_passwords[0].arn
    }
  )

  alb_enabled            = false
  cluster_id             = local.php_cluster_id
  cpu                    = 256
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "standalone"
  is_standalone_task     = true
  memory                 = 512
  name                   = local.fts_post_restore_reset_users_task_name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}

