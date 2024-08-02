locals {
  name     = "${var.name}.fifo"
  name_dlq = "${var.name}-deadletter.fifo"

  arn     = "arn:aws:sqs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:${local.name}"
  dlq_arn = "arn:aws:sqs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:${local.name_dlq}"
}
