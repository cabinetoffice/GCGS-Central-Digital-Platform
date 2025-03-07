resource "aws_sns_topic" "alerts_topic" {
  name = "${local.name_prefix}-${var.environment}-alerts"
  tags = var.tags
}

resource "aws_sns_topic" "alerts_topic_app_5xx" {
  name = "${local.name_prefix}-${var.environment}-alerts-app-5xx"
  tags = var.tags
}
