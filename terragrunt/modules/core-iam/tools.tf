resource "aws_iam_role" "tools" {
  name               = "${local.name_prefix}-tools"
  assume_role_policy = data.aws_iam_policy_document.tools_assume.json

  tags = var.tags
}
