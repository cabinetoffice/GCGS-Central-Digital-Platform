data "aws_iam_policy_document" "terraform_product_observability" {
  statement {
    actions = [
      "cloudwatch:Delete*",
      "cloudwatch:Describe*",
      "cloudwatch:Get*",
      "cloudwatch:List*",
      "cloudwatch:Put*",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:cloudwatch::${data.aws_caller_identity.current.account_id}:dashboard/${local.name_prefix}-*",
      "arn:aws:cloudwatch:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:alarm:${local.name_prefix}-*",
      "arn:aws:cloudwatch:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:alarm:canary/*/${local.name_prefix}-*",
    ]
  }

  statement {
    actions = [
      "logs:AssociateKmsKey",
      "logs:Create*",
      "logs:Delete*",
      "logs:List*",
      "logs:Put*",
      "logs:TagResource",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:logs:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:log-group:/${local.name_prefix}*",
      "arn:aws:logs:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:log-group:/aws/rds/cluster/${local.name_prefix}*",
      "arn:aws:logs:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:log-group:/aws/rds/proxy/${local.name_prefix}*",
      "arn:aws:logs:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:log-group:/aws/vendedlogs/${local.name_prefix}*",
      "arn:aws:logs:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:log-group:/ecs*",
      "arn:aws:logs:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:log-group:aws-waf-logs-*",
      "arn:aws:logs:us-east-1:${data.aws_caller_identity.current.account_id}:log-group:aws-waf-logs-*",
    ]
  }
}
