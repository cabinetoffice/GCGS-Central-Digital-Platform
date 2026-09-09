resource "aws_kms_key" "ecs_cloudwatch" {
  description             = "ECS ${local.name_prefix} for Cloudwatch log-group"
  deletion_window_in_days = 7
  key_usage               = "ENCRYPT_DECRYPT"

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-ecs-cloudwatch"
    }
  )
}

resource "aws_kms_alias" "ecs_cloudwatch" {
  name          = "alias/ecs/cloudwatch"
  target_key_id = aws_kms_key.ecs_cloudwatch.key_id
}

resource "aws_cloudwatch_log_group" "ecs" {
  name = "/${local.name_prefix}/ecs"

  retention_in_days = var.environment == "production" ? 0 : 90
  #   kms_key_id        = aws_kms_key.ecs_cloudwatch.arn

  tags = var.tags
}

resource "aws_cloudwatch_log_group" "ecs_php" {
  name = "/${local.name_prefix}/ecs-php"

  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

resource "aws_cloudwatch_log_group" "ecs_fts" {
  name = "/${local.name_prefix_fts}/ecs-fts"

  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

resource "aws_cloudwatch_log_group" "tasks" {
  for_each = toset(local.tasks)

  name = "/ecs/${each.value}"

  retention_in_days = var.environment == "production" ? 0 : 90
  #   kms_key_id = var.ecs_cloudwatch_kms_key_id

  tags = var.tags
}

resource "aws_cloudwatch_event_rule" "e2e_nightly_dev" {
  count = local.e2e_nightly_dev_enabled ? 1 : 0

  name                = "${local.name_prefix}-e2e-nightly-dev"
  schedule_expression = "cron(1 1 * * ? *)" # 01:01 UTC daily
  tags                = var.tags
}

resource "aws_cloudwatch_event_target" "e2e_nightly_dev" {
  count = local.e2e_nightly_dev_enabled ? 1 : 0

  rule      = aws_cloudwatch_event_rule.e2e_nightly_dev[0].name
  target_id = "e2e-nightly-dev"
  arn       = aws_sfn_state_machine.ecs_run_task[var.service_configs.e2e_nightly_dev.name].arn
  role_arn  = var.role_cloudwatch_events_arn

  depends_on = [
    aws_iam_role_policy_attachment.cloudwatch_event_invoke_deployer_step_function_attachment
  ]
}

