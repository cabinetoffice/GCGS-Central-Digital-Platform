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

resource "aws_cloudwatch_log_group" "tasks" {
  for_each = toset(local.tasks)

  name = "/ecs/${each.value}"

  retention_in_days = var.environment == "production" ? 0 : 90
  #   kms_key_id = var.ecs_cloudwatch_kms_key_id

  tags = var.tags
}

resource "aws_cloudwatch_event_rule" "ecr_push" {
  for_each = aws_ecr_repository.this

  name        = "${local.name_prefix}-ecr-push-to-${each.value.name}"
  description = "CloudWatch Event rule to detect ECR push events to ${each.value.name}"

  event_pattern = jsonencode(
    {
      "source" : ["aws.ecr"],
      "detail-type" : ["ECR Image Action"],
      "detail" : {
        "action-type" : ["PUSH"],
        "image-tag" : ["latest"],
        "repository-name" : [each.value.name]
        "result" : ["SUCCESS"],
      }
    }
  )

  tags = var.tags
}

resource "aws_cloudwatch_event_target" "trigger_service_deployment" {
  for_each = aws_cloudwatch_event_rule.ecr_push
  rule     = each.value.name
  arn      = each.key == "organisation-information-migrations" ? aws_sfn_state_machine.ecs_run_migration.arn : aws_sfn_state_machine.ecs_force_deploy[each.key].arn
  role_arn = var.role_cloudwatch_events_arn
}
