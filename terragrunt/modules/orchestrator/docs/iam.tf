resource "aws_iam_role" "docs_publisher" {
  name               = "${local.name_prefix}-docs-publisher"
  assume_role_policy = data.aws_iam_policy_document.github_oidc_assume.json
  tags               = var.tags
}

resource "aws_iam_policy" "docs_publisher" {
  name   = "${local.name_prefix}-docs-publisher"
  policy = data.aws_iam_policy_document.docs_publisher.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "docs_publisher" {
  policy_arn = aws_iam_policy.docs_publisher.arn
  role       = aws_iam_role.docs_publisher.name
}
