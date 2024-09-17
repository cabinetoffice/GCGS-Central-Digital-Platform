resource "aws_iam_policy" "canary" {
  name   = local.canary_name
  policy = data.aws_iam_policy_document.canary.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "canary_role_policy_attachment" {
  policy_arn = aws_iam_policy.canary.arn
  role       = var.role_canary_name
}
