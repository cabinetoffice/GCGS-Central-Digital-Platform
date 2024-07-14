resource "aws_iam_user" "github_user" {
  name = "${local.name_prefix}-github-user"
  tags = var.tags
}

resource "aws_iam_user_policy" "ecr_push_policy" {
  name   = "ECRPushPolicy"
  user   = aws_iam_user.github_user.name
  policy = data.aws_iam_policy_document.ecr_push_policy.json
}

resource "aws_iam_user_policy" "ssm_update_policy" {
  name   = "SSMUpdatePolicy"
  user   = aws_iam_user.github_user.name
  policy = data.aws_iam_policy_document.ssm_update_policy.json
}

resource "aws_iam_access_key" "github_user_access_key" {
  user = aws_iam_user.github_user.name
}
