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

variable "ecs_task_stopped_threshold" {
  description = "Stopped task count threshold for ECS restart alert"
  type        = number
  default     = 0
}

variable "rds_cpu_threshold" {
  description = "CPU threshold for RDS CPU alert"
  type        = number
  default     = 80
}

variable "rds_free_storage_threshold_bytes" {
  description = "Free storage threshold for RDS (bytes)"
  type        = number
  default     = 5 * 1024 * 1024 * 1024
}

variable "rds_freeable_memory_threshold_bytes" {
  description = "Freeable memory threshold for RDS (bytes)"
  type        = number
  default     = 512 * 1024 * 1024
}

variable "rds_connections_threshold" {
  description = "Database connections threshold for RDS"
  type        = number
  default     = 200
}

variable "rds_read_latency_threshold_seconds" {
  description = "Read latency threshold for RDS (seconds)"
  type        = number
  default     = 0.2
}

variable "rds_write_latency_threshold_seconds" {
  description = "Write latency threshold for RDS (seconds)"
  type        = number
  default     = 0.2
}
