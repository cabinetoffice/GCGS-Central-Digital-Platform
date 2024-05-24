resource "aws_ecs_cluster" "this" {
  name = local.name_prefix

  configuration {
    execute_command_configuration {
      kms_key_id = aws_kms_key.ecs_cloudwatch.arn
      logging    = "OVERRIDE"

      log_configuration {
        cloud_watch_encryption_enabled = true
        cloud_watch_log_group_name     = aws_cloudwatch_log_group.ecs.name
      }
    }
  }

  tags = var.tags
}
