resource "aws_iam_role" "canary" {

  assume_role_policy = data.aws_iam_policy_document.canary_assume.json
  name               = "${local.name_prefix}-canary"
  tags               = var.tags
}
