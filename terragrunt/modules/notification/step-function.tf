resource "aws_sfn_state_machine" "ses_logs_ingestor" {
  name     = "${local.name_prefix}-ses-logs-ingestor"
  role_arn = var.role_ses_logs_ingestor_step_function_arn
  tags     = var.tags

  definition = templatefile("${path.module}/templates/state-machine/ses-logs-ingestor.json.tftpl", {
    queue_url       = aws_sqs_queue.ses_json.id
    log_group_name  = aws_cloudwatch_log_group.ses_logs_ingestor.name
    logging_prefix  = local.logging_prefix
  })
}


