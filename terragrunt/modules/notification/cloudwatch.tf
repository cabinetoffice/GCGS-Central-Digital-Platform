resource "aws_kms_key" "ses_logs_ingestor" {
  description             = "SES ${local.name_prefix} for Cloudwatch log-group"
  deletion_window_in_days = 7
  key_usage               = "ENCRYPT_DECRYPT"

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-ses-cloudwatch"
    }
  )
}

resource "aws_kms_alias" "ses_logs_ingestor" {
  name          = "alias/ses/cloudwatch"
  target_key_id = aws_kms_key.ses_logs_ingestor.key_id
}

resource "aws_cloudwatch_log_group" "ses_logs_ingestor" {
  name = "/${local.name_prefix}/ses"

  retention_in_days = var.environment == "production" ? 90 : 30
  # kms_key_id        = aws_kms_key.ses_logs_ingestor.arn

  tags = var.tags
}

resource "aws_cloudwatch_event_rule" "ses_logs_ingestor" {
  name                = "${local.name_prefix}-ses-logs-ingestor"
  schedule_expression = "rate(1 minute)"
  tags                = var.tags
}

resource "aws_cloudwatch_event_target" "ses_logs_ingestor" {
  arn      = aws_sfn_state_machine.ses_logs_ingestor.arn
  rule     = aws_cloudwatch_event_rule.ses_logs_ingestor.name
  role_arn = var.role_ses_cloudwatch_events_arn
}
