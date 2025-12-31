data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_availability_zones" "current" {
  filter {
    name   = "region-name"
    values = [data.aws_region.current.region]
  }
}

data "aws_iam_policy_document" "elasticache_log_policy" {
  statement {
    sid    = "AllowElastiCacheSlowLogAccess"
    effect = "Allow"
    actions = [
      "logs:CreateLogStream",
      "logs:PutLogEvents"
    ]
    resources = [
      aws_cloudwatch_log_group.engine_log.arn,
      aws_cloudwatch_log_group.slow_log.arn
    ]
    principals {
      type        = "Service"
      identifiers = ["elasticache.amazonaws.com"]
    }
  }
}