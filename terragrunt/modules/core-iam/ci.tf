resource "aws_iam_role" "terraform" {
  assume_role_policy = data.aws_iam_policy_document.terraform_assume.json
  name               = "${local.name_prefix}-${var.environment}-terraform"
  tags               = var.tags
}

resource "aws_iam_policy" "terraform" {
  name   = "${local.name_prefix}-terraform"
  policy = data.aws_iam_policy_document.terraform.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "terraform" {
  policy_arn = aws_iam_policy.terraform.arn
  role       = aws_iam_role.terraform.name
}
