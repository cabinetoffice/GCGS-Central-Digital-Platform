data "aws_region" "virginia" {
  provider = aws.virginia
}

data "aws_iam_policy_document" "waf_manage_logs" {
  statement {
    sid    = "AllowWAFManageLogs"
    effect = "Allow"
    principals {
      identifiers = ["delivery.logs.amazonaws.com"]
      type        = "Service"
    }
    actions = [
      "logs:CreateLogStream",
      "logs:PutLogEvents"
    ]
    resources = ["${aws_cloudwatch_log_group.waf.arn}:*"]
    condition {
      test     = "ArnLike"
      values   = ["arn:aws:logs:${data.aws_region.virginia.region}:${data.aws_caller_identity.current.account_id}:*"]
      variable = "aws:SourceArn"
    }
    condition {
      test     = "StringEquals"
      values   = [tostring(data.aws_caller_identity.current.account_id)]
      variable = "aws:SourceAccount"
    }
  }
}

resource "aws_cloudwatch_log_group" "waf" {
  provider          = aws.virginia
  name              = local.waf_log_group_name
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

resource "aws_cloudwatch_log_resource_policy" "waf_manage_logs" {
  provider        = aws.virginia
  policy_document = data.aws_iam_policy_document.waf_manage_logs.json
  policy_name     = "AWSWAF-LOGS-CLOUDFRONT"
}

resource "aws_wafv2_web_acl_logging_configuration" "cloudfront" {
  count = var.cloudfront_enabled && var.waf_enabled && var.waf_logging_enabled ? 1 : 0

  provider                = aws.virginia
  log_destination_configs = [aws_cloudwatch_log_group.waf.arn]
  resource_arn            = aws_wafv2_web_acl.cloudfront[0].arn

  depends_on = [aws_cloudwatch_log_resource_policy.waf_manage_logs]
}
