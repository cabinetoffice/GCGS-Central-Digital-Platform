resource "aws_sqs_queue" "this_dlq" {
  name = local.name_dlq

  fifo_queue = true

  message_retention_seconds = var.message_retention_seconds

  policy = data.aws_iam_policy_document.access_policy["queue_dlq"].json

  tags = var.tags
}

resource "aws_sqs_queue" "this" {
  name = local.name

  fifo_queue                  = true
  content_based_deduplication = true

  delay_seconds              = var.delay_seconds
  max_message_size           = var.max_message_size
  message_retention_seconds  = var.message_retention_seconds
  receive_wait_time_seconds  = var.receive_wait_time_seconds
  visibility_timeout_seconds = var.visibility_timeout_seconds

  redrive_policy = jsonencode({
    deadLetterTargetArn = aws_sqs_queue.this_dlq.arn
    maxReceiveCount     = var.max_receive_count
  })

  policy = data.aws_iam_policy_document.access_policy["queue"].json

  tags = var.tags
}
