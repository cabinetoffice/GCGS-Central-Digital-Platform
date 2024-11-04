resource "aws_sfn_state_machine" "slack_unified_notification" {
  name     = "${local.name_prefix}-slack-unified-notification"
  role_arn = var.role_notification_step_function_arn
  tags     = var.tags

  definition = templatefile("${path.module}/templates/state-machine/send-slack-unified-notification.json.tftpl", {
    auth_connection_arn      = aws_cloudwatch_event_connection.slack_unified_notification.arn
    deployment_pipeline_name = var.deployment_pipeline_name
    dynamodb_table_name      = aws_dynamodb_table.pipeline_execution_timestamps.name
    slack_channel_id         = var.slack_channel_id //"C07V54GN52L"
    slack_post_endpoint      = "${local.slack_api_endpoint}/chat.postMessage"
    slack_update_endpoint    = "${local.slack_api_endpoint}/chat.update"
    ssm_service_version_name = var.ssm_service_version_name
  })
}
