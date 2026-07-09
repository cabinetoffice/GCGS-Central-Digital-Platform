output "queue_arn" {
  description = "ARN of the the SQS queue"
  value       = aws_sqs_queue.this.arn
}

output "queue_dlq_arn" {
  description = "ARN of the SQS dead-letter queue"
  value       = aws_sqs_queue.this_dlq.arn
}

output "queue_dlq_url" {
  description = "URL of the SQS dead-letter queue"
  value       = aws_sqs_queue.this_dlq.url
}

output "queue_names" {
  value = [local.name, local.name_dlq]
}

output "queue_url" {
  description = "URL of the SQS queue"
  value       = aws_sqs_queue.this.url
}
