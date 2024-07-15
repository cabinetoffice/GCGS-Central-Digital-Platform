data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

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


