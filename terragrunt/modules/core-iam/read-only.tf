resource "aws_iam_role" "read_only" {
  assume_role_policy = data.aws_iam_policy_document.read_only_assume.json
  name               = "${local.name_prefix}-${var.environment}-read-only"
  tags               = var.tags
}

resource "aws_iam_role_policy_attachment" "read_only" {
  policy_arn = "arn:aws:iam::aws:policy/ReadOnlyAccess"
  role       = aws_iam_role.read_only.name
}

