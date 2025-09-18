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
      "route53:ListHostedZones",
    ]

    resources = [
      "*",
    ]
  }

  statement {
    actions = [
      "iam:CreatePolicy",
      "iam:CreateServiceLinkedRole",
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
      "logs:CreateLogDelivery",
      "logs:DeleteLogDelivery",
      "logs:DeleteResourcePolicy",
      "logs:DescribeLogGroups",
      "logs:DescribeResourcePolicies",
      "logs:GetLogDelivery",
      "logs:ListLogDeliveries",
      "logs:PutResourcePolicy",
      "logs:UpdateLogDelivery",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:logs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:*",
    ]
    sid = "ManageLogs"
  }

  statement {
    actions = [
      "rds:AddTagsToResource",
      "rds:CreateDBInstanceReadReplica",
      "rds:DescribeDBInstances",
      "rds:RemoveTagsFromResource",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:rds:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:db:*",
    ]
    sid = "ManageRDS"
  }

  statement {
    actions = [
      "rds:DescribeGlobalClusters",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:rds::${data.aws_caller_identity.current.account_id}:global-cluster:*",
    ]
    sid = "ManageRDSCluster"
  }

  statement {
    actions = [
      "elasticloadbalancing:DescribeListenerAttributes",
      "elasticloadbalancing:DescribeListenerCertificates",
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
      "ssm:Delete*",
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

  statement {
    actions = [
      "states:ValidateStateMachineDefinition",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:states:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:stateMachine:*"
    ]
    sid = "ManageStateMachines"
  }

  statement {
    actions = [
      "wafv2:CreateWebACL",
      "wafv2:GetWebACLForResource",
      "wafv2:UpdateWebACL",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:wafv2:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:regional/managedruleset/*/*",
      "arn:aws:wafv2:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:regional/webacl/*/*",
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
