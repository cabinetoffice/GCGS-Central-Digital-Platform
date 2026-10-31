data "aws_iam_policy_document" "terraform_product_infra" {
  statement {
    actions = ["ec2:*"]
    effect  = "Allow"
    resources = [
      "arn:aws:ec2:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:*/${local.name_prefix}-*",
    ]
  }

  statement {
    actions = [
      "elasticloadbalancing:AddTags",
      "elasticloadbalancing:AddListenerCertificates",
      "elasticloadbalancing:Create*",
      "elasticloadbalancing:Delete*",
      "elasticloadbalancing:Describe*",
      "elasticloadbalancing:RemoveListenerCertificates",
      "elasticloadbalancing:RemoveTags",
      "elasticloadbalancing:Modify*",
      "elasticloadbalancing:SetRulePriorities",
      "elasticloadbalancing:SetWebACL",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:elasticloadbalancing:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:listener/app/cdp-*",
      "arn:aws:elasticloadbalancing:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:listener-rule/app/cdp-*",
      "arn:aws:elasticloadbalancing:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:loadbalancer/app/cdp-*",
      "arn:aws:elasticloadbalancing:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:targetgroup/*",
    ]
  }

  statement {
    actions = [
      "wafv2:AssociateWebACL",
      "wafv2:CreateIPSet",
      "wafv2:CreateWebACL",
      "wafv2:DeleteIPSet",
      "wafv2:DeleteLoggingConfiguration",
      "wafv2:DeleteWebACL",
      "wafv2:GetIPSet",
      "wafv2:GetLoggingConfiguration",
      "wafv2:GetWebACL",
      "wafv2:ListTagsForResource",
      "wafv2:PutLoggingConfiguration",
      "wafv2:TagResource",
      "wafv2:UpdateIPSet",
      "wafv2:UpdateWebACL",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:wafv2:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:regional/ipset/${local.name_prefix}-*/*",
      "arn:aws:wafv2:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:regional/webacl/${local.name_prefix}-*",
      "arn:aws:wafv2:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:regional/webacl/${local.name_prefix}-*/*",
      "arn:aws:wafv2:us-east-1:${data.aws_caller_identity.current.account_id}:global/ipset/${local.name_prefix}-*/*",
      "arn:aws:wafv2:us-east-1:${data.aws_caller_identity.current.account_id}:global/webacl/${local.name_prefix}-*/*",
    ]
  }
}
