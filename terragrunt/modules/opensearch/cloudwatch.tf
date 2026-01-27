resource "aws_cloudwatch_log_group" "index_slow" {
  name              = "/${local.name_prefix}/opensearch/index-slow"
  retention_in_days = 30
  tags              = var.tags
}

resource "aws_cloudwatch_log_group" "search_slow" {
  name              = "/${local.name_prefix}/opensearch/search-slow"
  retention_in_days = 30
  tags              = var.tags
}

resource "aws_cloudwatch_log_group" "es_application" {
  name              = "/${local.name_prefix}/opensearch/application"
  retention_in_days = 30
  tags              = var.tags
}

resource "aws_cloudwatch_log_resource_policy" "opensearch" {
  policy_name     = "${local.name_prefix}-opensearch-log-policy"
  policy_document = data.aws_iam_policy_document.opensearch_logs.json
}
