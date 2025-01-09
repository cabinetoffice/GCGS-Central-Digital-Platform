output "entity_verification_queue_arn" {
  description = "ARN of the entity-verification SQS queue"
  value       = module.entity_verification_queue.queue_arn
}

output "entity_verification_queue_url" {
  description = "URL of the entity-verification SQS queue"
  value       = module.entity_verification_queue.queue_url
}

output "organisation_queue_arn" {
  description = "ARN of the organisation SQS queue"
  value       = module.organisation_queue.queue_arn
}

output "organisation_queue_url" {
  description = "URL of the organisation SQS queue"
  value       = module.organisation_queue.queue_url
}

output "av_scanner_queue_arn" {
  description = "ARN of the AV scanner SQS queue"
  value       = module.av_scanner_queue.queue_arn
}

output "av_scanner_queue_url" {
  description = "URL of the AC scanner SQS queue"
  value       = module.av_scanner_queue.queue_url
}
