output "inbound_queue_url" {
  description = "URL of the inbound SQS queue"
  value       = aws_sqs_queue.ev_inbound.url
}

output "outbound_queue_url" {
  description = "URL of the outbound SQS queue"
  value       = aws_sqs_queue.ev_outbound.url
}
