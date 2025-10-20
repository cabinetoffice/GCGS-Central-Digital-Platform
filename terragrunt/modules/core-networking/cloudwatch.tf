resource "aws_cloudwatch_log_group" "waf" {
  name              = "aws-waf-logs-${local.name_prefix}"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

resource "aws_cloudwatch_log_group" "waf_php" {
  name              = "aws-waf-logs-${local.name_prefix_php}"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

resource "aws_cloudwatch_log_group" "waf_tools" {
  name              = "aws-waf-logs-${local.name_prefix}-tools"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}
