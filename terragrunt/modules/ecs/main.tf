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

  dynamic "setting" {
    for_each = var.is_production ? [0] : []
    content {
      name  = "containerInsights"
      value = "enabled"
    }
  }

  tags = var.tags
}

resource "aws_ecs_cluster" "that" {
  name = local.name_prefix_php

  configuration {
    execute_command_configuration {
      kms_key_id = aws_kms_key.ecs_cloudwatch.arn
      logging    = "OVERRIDE"

      log_configuration {
        cloud_watch_encryption_enabled = true
        cloud_watch_log_group_name     = aws_cloudwatch_log_group.ecs_php.name
      }
    }
  }

  dynamic "setting" {
    for_each = var.is_production ? [0] : []
    content {
      name  = "containerInsights"
      value = "enabled"
    }
  }

  tags = var.tags
}

resource "aws_ecs_cluster" "fts" {
  name = local.name_prefix_fts

  configuration {
    execute_command_configuration {
      kms_key_id = aws_kms_key.ecs_cloudwatch.arn
      logging    = "OVERRIDE"

      log_configuration {
        cloud_watch_encryption_enabled = true
        cloud_watch_log_group_name     = aws_cloudwatch_log_group.ecs_fts.name
      }
    }
  }

  dynamic "setting" {
    for_each = var.is_production ? [0] : []
    content {
      name  = "containerInsights"
      value = "enabled"
    }
  }

  tags = var.tags
}
