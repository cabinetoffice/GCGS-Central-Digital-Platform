data "aws_iam_policy_document" "terraform_global_cloudfront" {
  statement {
    actions = [
      "cloudfront:CreateDistribution",
      "cloudfront:CreateRealtimeLogConfig",
      "cloudfront:CreateOriginAccessControl",
      "cloudfront:CreateResponseHeadersPolicy",
      "cloudfront:DeleteDistribution",
      "cloudfront:DeleteRealtimeLogConfig",
      "cloudfront:DeleteOriginAccessControl",
      "cloudfront:DeleteResponseHeadersPolicy",
      "cloudfront:GetDistribution",
      "cloudfront:GetDistributionConfig",
      "cloudfront:GetOriginAccessControl",
      "cloudfront:GetRealtimeLogConfig",
      "cloudfront:GetResponseHeadersPolicy",
      "cloudfront:GetResponseHeadersPolicyConfig",
      "cloudfront:ListDistributions",
      "cloudfront:ListOriginAccessControls",
      "cloudfront:ListRealtimeLogConfigs",
      "cloudfront:ListResponseHeadersPolicies",
      "cloudfront:ListTagsForResource",
      "cloudfront:TagResource",
      "cloudfront:UntagResource",
      "cloudfront:UpdateDistribution",
      "cloudfront:UpdateOriginAccessControl",
      "cloudfront:UpdateRealtimeLogConfig",
      "cloudfront:UpdateResponseHeadersPolicy",
    ]
    effect = "Allow"
    resources = [
      "*"
    ]
    sid = "ManageCloudfront"
  }

  statement {
    actions = [
      "kinesis:AddTagsToStream",
      "kinesis:CreateStream",
      "kinesis:DeleteStream",
      "kinesis:DescribeStreamSummary",
      "kinesis:IncreaseStreamRetentionPeriod",
      "kinesis:DescribeStream",
      "kinesis:ListStreams",
      "kinesis:ListTagsForStream",
      "kinesis:RemoveTagsFromStream"
    ]
    effect = "Allow"
    resources = [
      "*"
    ]
    sid = "ManageKinesis"
  }

  statement {
    actions = [
      "wafv2:CreateWebACL",
      "wafv2:GetWebACLForResource",
      "wafv2:UpdateWebACL",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:wafv2:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:regional/managedruleset/*/*",
      "arn:aws:wafv2:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:regional/webacl/*/*",
      "arn:aws:wafv2:us-east-1:${data.aws_caller_identity.current.account_id}:global/managedruleset/*/*",
      "arn:aws:wafv2:us-east-1:${data.aws_caller_identity.current.account_id}:global/webacl/*/*",
    ]
    sid = "ManageWAF"
  }

  statement {
    actions = [
      "wafv2:DeleteLoggingConfiguration",
      "wafv2:PutLoggingConfiguration",
    ]
    effect = "Allow"
    resources = [
      "*",
    ]
    sid = "ManageWAFLoggingConfigurationAPI"
  }

  statement {
    actions = [
      "logs:CreateLogDelivery",
      "logs:DeleteLogDelivery",
      "logs:PutResourcePolicy",
      "logs:DescribeResourcePolicies",
      "logs:DescribeLogGroups"
    ]
    effect = "Allow"
    resources = [
      "*",
    ]
    sid = "ManageWAFWebACLLoggingCWL"
  }
}
