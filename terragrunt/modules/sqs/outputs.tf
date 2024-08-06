output "queue_arn" {
  description = "ARN of the the SQS queue"
  value       = aws_sqs_queue.this.arn
}

output "queue_url" {
  description = "URL of the SQS queue"
  value       = aws_sqs_queue.this.url
}
