resource "aws_iam_role" "teams_notifier" {
  name               = "${local.name_prefix}-teams-notifier"
  assume_role_policy = data.aws_iam_policy_document.lambda_assume.json
  tags               = var.tags
}

resource "aws_iam_policy" "teams_notifier" {
  name   = "${local.name_prefix}-teams-notifier"
  policy = data.aws_iam_policy_document.teams_notifier.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "teams_notifier" {
  role       = aws_iam_role.teams_notifier.name
  policy_arn = aws_iam_policy.teams_notifier.arn
}
