# Note!
# Resources in this file are shared with orchestrator/iam module

data "aws_codestarconnections_connection" "cabinet_office" {
  count = local.use_codestar_connection ? 1 : 0
  name  = "CabinetOffice"
}

data "aws_iam_policy_document" "ci_codebuild_assume_role_policy" {
  statement {
    actions = ["sts:AssumeRole"]
    principals {
      type        = "Service"
      identifiers = ["codebuild.amazonaws.com"]
    }
  }
}

data "aws_iam_policy_document" "ci_pipeline_assume_role_policy" {
  statement {
    actions = ["sts:AssumeRole"]
    principals {
      type        = "Service"
      identifiers = ["codepipeline.amazonaws.com"]
    }
  }
}

data "aws_iam_policy_document" "ci_build_generic" {

  statement {
    actions   = ["sts:AssumeRole"]
    resources = ["arn:aws:iam::${local.orchestrator_account_id}:role/cdp-sirsi-orchestrator-read-service-version"]
  }

  statement {
    actions = [
      "kms:GenerateDataKey",
      "kms:Decrypt"
    ]
    effect = "Allow"
    resources = [
      "*"
    ]
    sid = "ManageKms"
  }

  dynamic "statement" {
    for_each = local.use_codestar_connection ? [1] : []
    content {
      actions = [
        "codestar-connections:GetConnection",
        "codestar-connections:ListConnections",
        "codestar-connections:UseConnection"
      ]
      effect    = "Allow"
      resources = [data.aws_codestarconnections_connection.cabinet_office[0].arn]
    }
  }

  statement {
    sid = "AllowCloudWatchLogAccess"
    actions = [
      "logs:CreateLogGroup",
      "logs:CreateLogStream",
      "logs:PutLogEvents"
    ]

    resources = [
      "arn:aws:logs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:log-group:/aws/codebuild/*:log-stream:*",
    ]
  }

  statement {
    sid = "AllowEC2VPCAccess"
    actions = [
      "ec2:CreateNetworkInterface",
      "ec2:DescribeDhcpOptions",
      "ec2:DescribeNetworkInterfaces",
      "ec2:DeleteNetworkInterface",
      "ec2:DescribeSubnets",
      "ec2:DescribeSecurityGroups",
      "ec2:DescribeVpcs",
      "autoscaling:DescribeAutoScalingGroups",
      "autoscaling:UpdateAutoScalingGroup"
    ]

    resources = ["*"]
  }

  statement {
    sid = "AllowEC2NetworkInterfacePermissions"
    actions = [
      "ec2:CreateNetworkInterfacePermission"
    ]
    resources = [
      "arn:aws:ec2:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:network-interface/*"
    ]
    effect = "Allow"
    condition {
      test     = "StringEquals"
      variable = "ec2:AuthorizedService"

      values = ["codebuild.amazonaws.com"]
    }
  }

  statement {
    sid = "AllowECRAccess"
    actions = [
      "ecr:GetAuthorizationToken",
      "ecr:BatchCheckLayerAvailability",
      "ecr:GetDownloadUrlForLayer",
      "ecr:GetRepositoryPolicy",
      "ecr:DescribeRepositories",
      "ecr:ListImages",
      "ecr:DescribeImages",
      "ecr:BatchGetImage",
      "ecr:InitiateLayerUpload",
      "ecr:UploadLayerPart",
      "ecr:CompleteLayerUpload",
      "ecr:PutImage"
    ]
    resources = ["*"]
    effect    = "Allow"
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
      "arn:aws:s3:::cdp-sirsi-*",
      "arn:aws:s3:::cdp-sirsi-*/*"
    ]
    sid = "ManageProductS3Buckets"
  }

  statement {
    actions = [
      "S3:PutBucketPolicy",
    ]
    effect = "Allow"
    resources = [
      "*"
    ]
    sid = "ManageS3Buckets"
  }

}