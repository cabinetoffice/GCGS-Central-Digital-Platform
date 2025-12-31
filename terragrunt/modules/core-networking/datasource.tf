data "aws_caller_identity" "current" {}

data "aws_region" "current" {}


data "aws_iam_policy_document" "waf_manage_logs" {
  version = "2012-10-17"
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
      values   = ["arn:aws:logs:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:*"]
      variable = "aws:SourceArn"
    }
    condition {
      test     = "StringEquals"
      values   = [tostring(data.aws_caller_identity.current.account_id)]
      variable = "aws:SourceAccount"
    }
  }
}

data "aws_secretsmanager_secret_version" "waf_allowed_ips" {
  secret_id = "${local.name_prefix}-waf-allowed-ip-set"
}

data "aws_secretsmanager_secret_version" "waf_allowed_ips_tools" {
  secret_id = "${local.name_prefix}-waf-allowed-ip-set-tools"
}
