resource "aws_sns_topic" "alerts_topic" {
  name = "${local.name_prefix}-${var.environment}-alerts"
  tags = var.tags
}
