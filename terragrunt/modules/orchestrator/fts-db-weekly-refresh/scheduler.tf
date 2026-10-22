resource "aws_scheduler_schedule" "weekly_refresh" {
  name                         = "${local.name_prefix}-fts-db-weekly-refresh"
  schedule_expression          = "cron(0 0 ? * FRI *)"
  schedule_expression_timezone = var.schedule_timezone

  flexible_time_window {
    mode = "OFF"
  }

  target {
    arn      = "arn:aws:scheduler:::aws-sdk:codepipeline:startPipelineExecution"
    role_arn = aws_iam_role.scheduler.arn
    input    = jsonencode({ Name = aws_codepipeline.weekly_refresh.name })
  }
}
