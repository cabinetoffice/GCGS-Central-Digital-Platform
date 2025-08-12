data "aws_iam_policy_document" "terraform_product" {

  statement {
    actions = [
      "iam:*",
      "ecs:*"
    ]
    effect = "Allow"
    resources = [
      "arn:aws:iam::${data.aws_caller_identity.current.account_id}:role/${local.name_prefix}-*",
      "arn:aws:iam::${data.aws_caller_identity.current.account_id}:policy/${local.name_prefix}-*",
      "arn:aws:iam::${data.aws_caller_identity.current.account_id}:role/aws-service-role/*",
      "arn:aws:iam::${data.aws_caller_identity.current.account_id}:user/${local.name_prefix}-*"
    ]
    sid = "ManageProductIAMs"
  }

  statement {
    actions = [
      "s3:GetObject",
      "s3:GetObjectVersion"
    ]
    effect = "Allow"
    resources = [
      "arn:aws:s3:::aws-synthetics-library-eu-west-2/*"
    ]
    sid = "ManageCanaryAllowS3Access"
  }

  statement {
    actions = [
      "lambda:Add*",
      "lambda:Create*",
      "lambda:Get*",
      "lambda:Publish*",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:lambda:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:function:cwsyn-${local.name_prefix}-*",
      "arn:aws:lambda:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:layer:cwsyn-${local.name_prefix}-*",
      "arn:aws:lambda:${data.aws_region.current.name}:*:layer:Synthetics_*",
    ]
    sid = "ManageProductCanarysLambda"
  }

  statement {
    actions = [
      "dynamodb:*",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:dynamodb:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:table/${local.name_prefix}-*",
    ]
    sid = "ManageProductDynamoDB"
  }

  statement {
    actions = [
      "synthetics:Create*",
      "synthetics:DeleteCanary",
      "synthetics:Get*",
      "synthetics:Start*",
      "synthetics:Stop*",
      "synthetics:Tag*",
      "synthetics:Update*",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:synthetics:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:canary:${local.name_prefix}-*"
    ]
    sid = "ManageProductCanary"
  }

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
      "arn:aws:cloudwatch:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:alarm:${local.name_prefix}-*",
      "arn:aws:cloudwatch:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:alarm:canary/*/${local.name_prefix}-*"
    ]
    sid = "ManageProductCloudwatch"
  }

  statement {
    actions = ["ec2:*"]
    effect  = "Allow"
    resources = [
      "arn:aws:ec2:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:*/${local.name_prefix}-*"
    ]
    sid = "ManageProductEC2"
  }

  statement {
    actions = [
      "ecr:*",
      "ecs:*"
    ]
    effect = "Allow"
    resources = [
      "arn:aws:ecr:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:repository/cdp-*",
      "arn:aws:ecs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:cluster/cdp-*",
      "arn:aws:ecs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:service/cdp-*",
      "arn:aws:ecs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:task-definition/app*",
      "arn:aws:ecs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:task-definition/db*",
      "arn:aws:ecs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:task-definition/standalone-*",
      "arn:aws:ecs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:task-definition/telemetry*",
      "arn:aws:ecs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:task-definition/tools*",
    ]
    sid = "ManageProductECS"
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
      "arn:aws:logs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:log-group:/${local.name_prefix}*",
      "arn:aws:logs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:log-group:/aws/vendedlogs/${local.name_prefix}*",
      "arn:aws:logs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:log-group:/ecs*",
      "arn:aws:logs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:log-group:aws-waf-logs-*",
    ]
    sid = "ManageProductLogs"
  }

  statement {
    actions = [
      "events:CreateConnection",
      "events:DescribeConnection",
      "events:DeleteConnection",
      "events:DeleteRule",
      "events:DescribeRule",
      "events:ListTagsForResource",
      "events:ListTargetsByRule",
      "events:PutRule",
      "events:PutTargets",
      "events:RemoveTargets",
      "events:TagResource",
      "events:UpdateConnection",
      "iam:PassRole",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:events:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:connection/${local.name_prefix}-*",
      "arn:aws:events:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:rule/${local.name_prefix}-*",
      "arn:aws:events:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:rule/StepFunctionsGetEventsForStepFunctionsExecutionRule",
    ]
    sid = "ManageProductEvents"
  }

  statement {
    actions = [
      "secretsmanager:*",
      "kms:Encrypt",
      "kms:Decrypt",
      "kms:ReEncrypt*",
      "kms:CreateGrant",
      "kms:DescribeKey",
      "kms:UpdateAlias",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:kms:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:alias/rds-${local.name_prefix}-*",
      "arn:aws:kms:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:key*",
      "arn:aws:secretsmanager:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:secret:${local.name_prefix}*",
      "arn:aws:secretsmanager:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:secret:events!connection/${local.name_prefix}-*",
      "arn:aws:secretsmanager:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:secret:rds!cluster*",
      "arn:aws:secretsmanager:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:secret:rds!db*",
      "arn:aws:secretsmanager:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:secret:rds-db*",
    ]
    sid = "ManageProductSecrets"
  }

  statement {
    actions = [
      "elasticloadbalancing:AddTags",
      "elasticloadbalancing:AddListenerCertificates",
      "elasticloadbalancing:Create*",
      "elasticloadbalancing:Delete*",
      "elasticloadbalancing:Describe*",
      "elasticloadbalancing:RemoveListenerCertificates",
      "elasticloadbalancing:Modify*",
      "elasticloadbalancing:SetRulePriorities",
      "elasticloadbalancing:SetWebACL",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:elasticloadbalancing:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:listener/app/cdp-*",
      "arn:aws:elasticloadbalancing:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:listener-rule/app/cdp-*",
      "arn:aws:elasticloadbalancing:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:loadbalancer/app/cdp-*",
      "arn:aws:elasticloadbalancing:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:targetgroup/cdp-*",
    ]
    sid = "ManageProductLBs"
  }

  statement {
    actions = [
      "states:CreateStateMachine",
      "states:Delete*",
      "states:Describe*",
      "states:List*",
      "states:StartExecution",
      "states:TagResource",
      "states:UpdateStateMachine",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:states:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:stateMachine:${local.name_prefix}-*"
    ]
    sid = "ManageProductStateMachines"
  }

  statement {
    actions = [
      "codebuild:*",
      "codepipeline:*",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:codebuild:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:project/${local.name_prefix}-*",
      "arn:aws:codepipeline:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:${local.name_prefix}-*"
    ]
    sid = "ManageProductCodebuild"
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
      "arn:aws:wafv2:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:regional/webacl/${local.name_prefix}-*",
      "arn:aws:wafv2:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:regional/webacl/${local.name_prefix}-*/*",
      "arn:aws:wafv2:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:regional/ipset/${local.name_prefix}-*/*",
    ]
    sid = "ManageProductWAF"
  }

  statement {
    actions = [
      "SNS:CreateTopic",
      "SNS:DeleteTopic",
      "SNS:GetSubscriptionAttributes",
      "SNS:GetTopicAttributes",
      "SNS:ListTagsForResource",
      "SNS:SetTopicAttributes",
      "SNS:Subscribe",
      "SNS:TagResource",
      "SNS:Unsubscribe",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:sns:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:${local.name_prefix}-*",
    ]
    sid = "ManageProductSNS"
  }

}
