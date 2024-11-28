resource "aws_cloudwatch_log_group" "engine_log" {
  name              = "/aws/vendedlogs/${local.name_prefix}/elasticache/engine-log"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

resource "aws_cloudwatch_log_group" "slow_log" {
  name              = "/aws/vendedlogs/${local.name_prefix}/elasticache/slow-log"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

resource "aws_cloudwatch_log_resource_policy" "elasticache_logs" {
  policy_name     = "ElastiCacheLogAccess"
  policy_document = data.aws_iam_policy_document.elasticache_log_policy.json
}
