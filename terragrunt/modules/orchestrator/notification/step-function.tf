resource "aws_sfn_state_machine" "slack_notification_middleman" {
  name     = "${local.name_prefix}-slack-notification-middleman"
  role_arn = var.role_notification_step_function_arn
  tags     = var.tags

  definition = templatefile("${path.module}/templates/state-machine/trigger-slack-notification.json.tftpl", {
    slack_notification_arn = aws_sfn_state_machine.slack_notification.arn
  })
}

resource "aws_sfn_state_machine" "slack_notification" {
  name     = "${local.name_prefix}-slack-notification"
  role_arn = var.role_notification_step_function_arn
  tags     = var.tags

  definition = templatefile("${path.module}/templates/state-machine/send-slack-notification.json.tftpl", {
    auth_connection_arn                  = aws_cloudwatch_event_connection.teams_webhook.arn
    teams_webhook_secret_arn              = local.teams_webhook_secret_arn
  })
}
