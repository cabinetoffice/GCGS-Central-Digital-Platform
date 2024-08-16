resource "aws_iam_role" "pen_testing" {
  assume_role_policy = data.aws_iam_policy_document.assume_pen_testing.json
  name               = "${local.name_prefix}-pen-testing"
  tags               = var.tags
}

resource "aws_iam_policy" "pen_testing_self_management" {
  name   = "${local.name_prefix}-pen-testing-self-management"
  policy = data.aws_iam_policy_document.pen_testing_self_management.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "pen_testing_self_management" {
  policy_arn = aws_iam_policy.pen_testing_self_management.arn
  role       = aws_iam_role.pen_testing.name
}

resource "aws_iam_role_policy_attachment" "pen_testing_readonly" {
  policy_arn = "arn:aws:iam::aws:policy/ReadOnlyAccess"
  role       = aws_iam_role.pen_testing.name
}

resource "aws_iam_role_policy_attachment" "pen_testing_security_audit" {
  policy_arn = "arn:aws:iam::aws:policy/SecurityAudit"
  role       = aws_iam_role.pen_testing.name
}
