data "aws_iam_policy_document" "terraform_product_ci" {
  statement {
    actions = [
      "codebuild:*",
      "codepipeline:*",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:codebuild:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:project/${local.name_prefix}-*",
      "arn:aws:codepipeline:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:${local.name_prefix}-*",
    ]
  }
}
