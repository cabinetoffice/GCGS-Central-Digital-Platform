resource "aws_sfn_state_machine" "slack_notification" {
  name     = "${local.name_prefix}-slack-notification"
  role_arn = var.role_notification_step_function_arn
  tags     = var.tags

  definition = templatefile("${path.module}/templates/state-machine/send-slack-notification.json.tftpl", {
    api_endpoint             = data.aws_secretsmanager_secret_version.slack_api_endpoint.secret_string
    auth_connection_arn      = aws_cloudwatch_event_connection.slack.arn
    ssm_service_version_name = var.ssm_service_version_name
    deployment_pipeline_name = var.deployment_pipeline_name
  })
}
