data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_iam_role" "terraform" {
  name = "${local.name_prefix}-${var.environment}-terraform"
}

data "aws_iam_policy_document" "ecr_push_policy" {
  statement {
    actions = [
      "ecr:BatchCheckLayerAvailability",
      "ecr:BatchGetImage",
      "ecr:CompleteLayerUpload",
      "ecr:GetDownloadUrlForLayer",
      "ecr:InitiateLayerUpload",
      "ecr:PutImage",
      "ecr:UploadLayerPart",
    ]
    resources = [
      "arn:aws:ecr:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:repository/cdp-*"
    ]
    effect = "Allow"
  }

  statement {
    actions = [
      "ecr:GetAuthorizationToken",
    ]
    resources = [
      "*"
    ]
    effect = "Allow"
  }
}

data "aws_iam_policy_document" "ssm_update_policy" {
  statement {
    actions = [
      "ssm:PutParameter",
      "ssm:GetParameter",
      "ssm:DeleteParameter"
    ]
    resources = ["*"] # @TODO: (ABN) Limit me
    effect    = "Allow"
  }
}


data "aws_iam_policy_document" "orchestrator_read_service_version_assume_role" {
  statement {
    actions = ["sts:AssumeRole"]
    principals {
      type        = "AWS"
      identifiers = local.combined_roles
    }
  }
}


data "aws_iam_policy_document" "orchestrator_read_service_version" {
  statement {
    actions   = ["ssm:GetParameter"]
    resources = ["arn:aws:ssm:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:parameter/${local.name_prefix}-service-version"]
  }
}
