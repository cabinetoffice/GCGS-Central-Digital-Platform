data "aws_region" "current" {}

data "aws_caller_identity" "current" {}

data "aws_iam_policy_document" "ecs_opensearch_access" {
  statement {
    sid     = "OpenSearchHttpAccess"
    effect  = "Allow"
    actions = ["es:ESHttp*"]

    resources = [
      local.domain_arn,
      "${local.domain_arn}/*"
    ]
  }
}

data "aws_iam_policy_document" "opensearch_access" {
  statement {
    sid    = "AllowHttpFromApprovedPrincipals"
    effect = "Allow"

    actions = [
      "es:ESHttp*"
    ]

    principals {
      type        = "AWS"
      identifiers = local.opensearch_access_principals
    }

    resources = [
      local.domain_arn,
      "${local.domain_arn}/*"
    ]
  }
}

data "aws_iam_policy_document" "opensearch_logs" {
  statement {
    sid    = "AllowOpenSearchToWriteLogs"
    effect = "Allow"

    actions = [
      "logs:PutLogEvents",
      "logs:CreateLogStream",
    ]

    principals {
      type        = "Service"
      identifiers = ["es.amazonaws.com"]
    }

    resources = [
      aws_cloudwatch_log_group.index_slow.arn,
      "${aws_cloudwatch_log_group.index_slow.arn}:*",
      aws_cloudwatch_log_group.search_slow.arn,
      "${aws_cloudwatch_log_group.search_slow.arn}:*",
      aws_cloudwatch_log_group.es_application.arn,
      "${aws_cloudwatch_log_group.es_application.arn}:*",
    ]
  }
}


