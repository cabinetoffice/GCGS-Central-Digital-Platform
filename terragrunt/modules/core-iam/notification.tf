resource "aws_iam_role" "notification_step_function" {
  name               = "${local.name_prefix}-notification-step-function"
  assume_role_policy = data.aws_iam_policy_document.notification_step_function_assume.json

  tags = var.tags
}