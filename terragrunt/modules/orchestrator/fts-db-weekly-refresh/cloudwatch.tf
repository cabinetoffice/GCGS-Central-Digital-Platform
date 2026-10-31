resource "aws_cloudwatch_log_group" "weekly_refresh" {
  name              = "/${local.name_prefix}/fts-db-weekly-refresh"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}
