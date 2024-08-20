# Note!
# Resources in this file are shared with orchestrator/iam module

data "aws_iam_policy_document" "terraform_assume" {
  statement {
    sid     = "AllowTerraformOperatorsAssumeRole"
    actions = ["sts:AssumeRole"]
    principals {
      type        = "AWS"
      identifiers = var.terraform_operators
    }
    condition {
      test     = "Bool"
      values   = [true]
      variable = "aws:MultiFactorAuthPresent"
    }
  }

  statement {
    sid     = "AllowOrchestratorCodebuildAssumeRole"
    actions = ["sts:AssumeRole"]
    principals {
      type        = "AWS"
      identifiers = ["arn:aws:iam::${local.orchestrator_account_id}:role/${local.name_prefix}-orchestrator-ci-codebuild"]
    }
  }
}

data "aws_iam_policy_document" "terraform_assume_orchestrator_role" {
  statement {
    effect    = "Allow"
    actions   = ["sts:AssumeRole"]
    resources = ["arn:aws:iam::${local.orchestrator_account_id}:role/${local.name_prefix}-orchestrator-read-service-version"]
  }
}

data "aws_iam_policy_document" "terraform" {

  statement {
    actions = ["dynamodb:*"]
    effect  = "Allow"
    resources = [
      "arn:aws:dynamodb:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:table/terraform-locks"
    ]
    sid = "ManageTerraformLock"
  }

}

data "aws_iam_policy_document" "terraform_global" {

  statement {
    sid = "ManageR53"
    actions = [
      "route53:ChangeResourceRecordSets",
      "route53:ChangeTagsForResource",
      "route53:CreateHostedZone",
      "route53:DeleteHostedZone",
      "route53:GetChange",
      "route53:GetHostedZone",
      "route53:GetHostedZone",
      "route53:ListResourceRecordSets",
      "route53:ListTagsForResource",
    ]

    resources = [
      "*",
    ]
  }

  statement {
    actions = [
      "iam:CreatePolicy",
      "iam:GetPolicy",
      "iam:GetPolicyVersion",
      "iam:TagPolicy",
    ]
    effect = "Allow"
    resources = [
      "*",
    ]
    sid = "ManageIAMs"
  }

  statement {
    actions = [
      "acm:AddTagsToCertificate",
      "acm:DeleteCertificate",
      "acm:DescribeCertificate",
      "acm:ListTagsForCertificate",
      "acm:RequestCertificate",
      "iam:PassRole",
    ]
    effect = "Allow"
    resources = [
      "*",
    ]
    sid = "ManageACMs"
  }

  statement {
    actions = [
      "apigateway:DELETE",
      "apigateway:GET",
      "apigateway:PATCH",
      "apigateway:POST",
      "apigateway:PUT",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:apigateway:${data.aws_region.current.name}:*",
    ]
    sid = "ManageAPIGateway"
  }

  statement {
    actions = [
      "ec2:AllocateAddress",
      "ec2:AssociateRouteTable",
      "ec2:AttachInternetGateway",
      "ec2:AuthorizeSecurityGroupEgress",
      "ec2:AuthorizeSecurityGroupIngress",
      "ec2:CreateInternetGateway",
      "ec2:CreateNatGateway",
      "ec2:CreateRoute",
      "ec2:CreateRouteTable",
      "ec2:CreateSecurityGroup",
      "ec2:CreateSubnet",
      "ec2:CreateTags",
      "ec2:CreateVpc",
      "ec2:CreateVpcEndpoint",
      "ec2:DeleteInternetGateway",
      "ec2:DeleteNatGateway",
      "ec2:DeleteRoute",
      "ec2:DeleteRouteTable",
      "ec2:DeleteSecurityGroup",
      "ec2:DeleteSubnet",
      "ec2:DeleteVpc",
      "ec2:DeleteVpcEndpoints",
      "ec2:DescribeAccountAttributes",
      "ec2:DescribeAddresses",
      "ec2:DescribeAddressesAttribute",
      "ec2:DescribeInternetGateways",
      "ec2:DescribeNatGateways",
      "ec2:DescribeNetworkInterfaces",
      "ec2:DescribePrefixLists",
      "ec2:DescribeRouteTables",
      "ec2:DescribeSecurityGroupRules",
      "ec2:DescribeSecurityGroups",
      "ec2:DescribeSubnets",
      "ec2:DescribeVpcAttribute",
      "ec2:DescribeVpcEndpoints",
      "ec2:DescribeVpcs",
      "ec2:DetachInternetGateway",
      "ec2:ModifySubnetAttribute",
      "ec2:ModifyVpcAttribute",
      "ec2:ModifyVpcEndpoint",
      "ec2:ReleaseAddress",
      "ec2:RevokeSecurityGroupEgress",
      "ec2:RevokeSecurityGroupIngress",
      "ec2:UpdateSecurityGroupRuleDescriptionsEgress",
      "ec2:UpdateSecurityGroupRuleDescriptionsIngress",
    ]
    effect = "Allow"
    resources = [
      "*"
    ]
    sid = "ManageEC2"
  }

  statement {
    actions = [
      "kms:Create*",
      "kms:Delete*",
      "kms:Get*",
      "kms:List*",
      "kms:ScheduleKeyDeletion",
      "kms:TagResource",
      "kms:UpdateKeyDescription",
    ]
    effect = "Allow"
    resources = [
      "*"
    ]
    sid = "ManageKms"
  }

  statement {
    actions = [
      "cloudfront:UpdateDistribution",
    ]
    effect = "Allow"
    resources = [
      "*"
    ]
    sid = "ManageCloudfront"
  }

  statement {
    actions = [
      "kms:*",
      "kms:Decrypt",
      "kms:ReEncrypt*",
      "kms:CreateGrant",
      "kms:DescribeKey"
    ]
    effect    = "Allow"
    resources = ["*"]
    sid       = "ManageKmsViaService"

    condition {
      # @todo (ABN) restricted for CI/CD, test it when pipeline is in place
      test     = "ForAnyValue:StringEquals"
      values   = [data.aws_caller_identity.current.account_id]
      variable = "kms:CallerAccount"
    }
    condition {
      test     = "ForAnyValue:StringEquals"
      values   = ["secretsmanager.${data.aws_region.current.name}.amazonaws.com"]
      variable = "kms:ViaService"
    }
  }

  statement {
    actions = [
      "ecr:BatchGetImage",
      "ecr:GetAuthorizationToken",
      "ecr:GetDownloadUrlForLayer",
      "ecs:Create*",
      "ecs:DeregisterTaskDefinition",
      "ecs:DescribeTaskDefinition",
      "ecs:TagResource",
    ]
    effect = "Allow"
    resources = [
      "*"
    ]
    sid = "ManageECS"
  }

  statement {
    actions = [
      "logs:DescribeLogGroups",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:logs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:*",
    ]
    sid = "ManageLogs"
  }

  statement {
    actions = [
      "rds:DescribeDBInstances",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:rds:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:db:*",
    ]
    sid = "ManageRDS"
  }

  statement {
    actions = [
      "elasticloadbalancing:DescribeListeners",
      "elasticloadbalancing:DescribeLoadBalancerAttributes",
      "elasticloadbalancing:DescribeLoadBalancers",
      "elasticloadbalancing:DescribeRules",
      "elasticloadbalancing:DescribeTags",
      "elasticloadbalancing:DescribeTargetGroupAttributes",
      "elasticloadbalancing:DescribeTargetGroups",
    ]
    effect = "Allow"
    resources = [
      "*",
    ]
    sid = "ManageLBs"
  }

  statement {
    actions = [
      "codestar-connections:Describe*",
      "codestar-connections:Get*",
      "codestar-connections:List*",
      "codestar-connections:PassConnection",
    ]
    effect = "Allow"
    resources = [
      "*"
    ]
    sid = "ManageCodestar"
  }

  statement {
    actions = [
      "S3:PutBucketPolicy",
      "s3:DeleteBucketPolicy",
      "s3:GetBucketPolicy",
      "s3:GetBucketPolicyStatus",
    ]
    effect = "Allow"
    resources = [
      "*"
    ]
    sid = "ManageBucketPolicy"
  }

  statement {
    actions = [
      "cognito-idp:Admin*",
      "cognito-idp:Create*",
      "cognito-idp:Delete*",
      "cognito-idp:Describe*",
      "cognito-idp:Get*",
      "cognito-idp:Set*",
      "cognito-idp:Update*",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:cognito-idp:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:userpool/*"
    ]
    sid = "ManageCognito"
  }

  statement {
    actions = [
      "cognito-idp:DescribeUserPoolDomain",
      "cognito-idp:ListUserPools",
    ]
    effect = "Allow"
    resources = [
      "*"
    ]
    sid = "ManageCognitoGlobal"
  }

  statement {
    actions = [
      "ssm:AddTagsToResource",
      "ssm:Describe*",
      "ssm:Get*",
      "ssm:List*",
      "ssm:PutParameter",
    ]
    effect = "Allow"
    resources = [
      "*"
    ]
    sid = "ManageSSMGlobal"
  }

}

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
      "cloudwatch:Delete*",
      "cloudwatch:Get*",
      "cloudwatch:Put*",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:cloudwatch::${data.aws_caller_identity.current.account_id}:dashboard/${local.name_prefix}-*"
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
    ]
    effect = "Allow"
    resources = [
      "arn:aws:logs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:log-group:/ecs*",
      "arn:aws:logs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:log-group:/cdp-*",
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
      "rds:AddTagsToResource",
      "rds:Create*",
      "rds:Delete*",
      "rds:Describe*",
      "rds:List*",
      "rds:Modify*",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:rds:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:db:cdp-*",
      "arn:aws:rds:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:pg:cdp-*",
      "arn:aws:rds:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:subgrp:cdp-*",
    ]
    sid = "ManageProductRDS"
  }

  statement {
    actions = [
      "secretsmanager:*",
      "kms:Encrypt",
      "kms:Decrypt",
      "kms:ReEncrypt*",
      "kms:CreateGrant",
      "kms:DescribeKey",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:kms:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:key*",
      "arn:aws:secretsmanager:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:secret:events!connection/cdp-sirsi-*",
      "arn:aws:secretsmanager:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:secret:rds!db*",
      "arn:aws:secretsmanager:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:secret:${local.name_prefix}*",
    ]
    sid = "ManageProductSecrets"
  }

  statement {
    actions = [
      "elasticloadbalancing:AddTags",
      "elasticloadbalancing:Create*",
      "elasticloadbalancing:Delete*",
      "elasticloadbalancing:Describe*",
      "elasticloadbalancing:Modify*",
      "elasticloadbalancing:SetRulePriorities",
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
      "states:UpdateStateMachine"
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
      "s3:Create*",
      "s3:Delete*",
      "s3:Get*",
      "s3:List*",
      "s3:Put*",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:s3:::${local.name_prefix}-*",
      "arn:aws:s3:::${local.name_prefix}-*/*"
    ]
    sid = "ManageProductS3Buckets"
  }

  statement {
    actions = [
      "sqs:*",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:sqs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:${local.name_prefix}-*"
    ]
    sid = "ManageProductSQS"
  }



}
