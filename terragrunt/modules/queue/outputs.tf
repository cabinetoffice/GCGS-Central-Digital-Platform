output "entity_verification_queue_arn" {
  description = "ARN of the entity-verification SQS queue"
  value       = module.entity_verification_queue.queue_arn
}

output "entity_verification_queue_url" {
  description = "URL of the entity-verification SQS queue"
  value       = module.entity_verification_queue.queue_url
}

output "healthcheck_queue_arn" {
  description = "ARN of the health-check SQS queue"
  value       = module.healthcheck_queue.queue_arn
}

output "healthcheck_queue_url" {
  description = "URL of the health-check SQS queue"
  value       = module.healthcheck_queue.queue_url
}


output "organisation_queue_arn" {
  description = "ARN of the organisation SQS queue"
  value       = module.organisation_queue.queue_arn
}

output "organisation_queue_url" {
  description = "URL of the organisation SQS queue"
  value       = module.organisation_queue.queue_url
}
