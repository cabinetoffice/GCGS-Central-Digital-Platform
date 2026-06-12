resource "aws_sfn_state_machine" "teams_notification_middleman" {
  name     = "${local.name_prefix}-teams-notification-middleman"
  role_arn = var.role_notification_step_function_arn
  tags     = var.tags

  definition = templatefile("${path.module}/templates/state-machine/trigger-teams-notification.json.tftpl", {
    teams_notification_arn = aws_sfn_state_machine.teams_notification.arn
  })
}

resource "aws_sfn_state_machine" "teams_notification" {
  name     = "${local.name_prefix}-teams-notification"
  role_arn = var.role_notification_step_function_arn
  tags     = var.tags

  definition = templatefile("${path.module}/templates/state-machine/send-teams-notification.json.tftpl", {
    teams_notifier_lambda_arn = var.teams_notifier_lambda_arn
  })
}
