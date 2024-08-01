output "entity_verification_queue_arn" {
  description = "ARN of the entity-verification SQS queue"
  value       = aws_sqs_queue.entity_verification.arn
}

output "entity_verification_queue_url" {
  description = "URL of the entity-verification SQS queue"
  value       = aws_sqs_queue.entity_verification.url
}

output "organisation_queue_arn" {
  description = "ARN of the organisation SQS queue"
  value       = aws_sqs_queue.organisation.arn
}

output "organisation_queue_url" {
  description = "URL of the organisation SQS queue"
  value       = aws_sqs_queue.organisation.url
}
