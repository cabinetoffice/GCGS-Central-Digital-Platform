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
    auth_connection_arn                  = aws_cloudwatch_event_connection.teams_webhook.arn
    teams_webhook_secret_arn              = local.teams_webhook_secret_arn
  })
}
