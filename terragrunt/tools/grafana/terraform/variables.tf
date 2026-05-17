variable "grafana_url" {
  description = "Base URL for Grafana (e.g. https://grafana.<domain>)"
  type        = string
}

variable "grafana_token" {
  description = "Grafana API token"
  type        = string
  sensitive   = true
}

variable "environment" {
  description = "Environment name (development, staging, integration, production)"
  type        = string
}

variable "cloudwatch_account_id" {
  description = "AWS account ID for CloudWatch logs/metrics"
  type        = string
}

variable "cloudwatch_assume_role_arn" {
  description = "IAM role ARN for Grafana to assume when querying CloudWatch"
  type        = string
}

variable "teams_webhook_url" {
  description = "Microsoft Teams webhook URL"
  type        = string
  sensitive   = true
  default     = ""
}

variable "alert_contact_point_name" {
  description = "Name for the Teams contact point"
  type        = string
  default     = "Microsoft Teams"
}

variable "ecs_cpu_threshold" {
  description = "CPU threshold for ECS CPU alert"
  type        = number
  default     = 80
}

variable "ecs_memory_threshold" {
  description = "Memory threshold for ECS memory alert"
  type        = number
  default     = 80
}
