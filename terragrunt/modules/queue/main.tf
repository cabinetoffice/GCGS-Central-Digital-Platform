resource "aws_sqs_queue" "ev_inbound" {
  name = "${local.name_prefix}-ev-inbound"

  delay_seconds              = var.delay_seconds
  max_message_size           = var.max_message_size
  message_retention_seconds  = var.message_retention_seconds
  receive_wait_time_seconds  = var.receive_wait_time_seconds
  visibility_timeout_seconds = var.visibility_timeout_seconds

  tags = var.tags
}

resource "aws_sqs_queue" "ev_outbound" {
  name = "${local.name_prefix}-ev-outbound"

  delay_seconds              = var.delay_seconds
  max_message_size           = var.max_message_size
  message_retention_seconds  = var.message_retention_seconds
  receive_wait_time_seconds  = var.receive_wait_time_seconds
  visibility_timeout_seconds = var.visibility_timeout_seconds

  tags = var.tags
}
