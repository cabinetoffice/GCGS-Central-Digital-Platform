variable "backup_retention_period" {
  description = "The number of days to retain backups for"
  type        = number
  default     = 7
}

variable "copy_tags_to_snapshot" {
  description = "Whether copy all Instance tags to snapshots"
  type        = bool
  default     = false
}

variable "db_name" {
  description = "Data base name"
  type        = string
}

variable "db_parameters_cluster" {
  description = "A map of database parameters to apply to the RDS DB parameter group. Keys are parameter names, and values are the desired settings."
  type        = map(string)
  default     = {}
}

variable "db_parameters_instance" {
  description = "A map of database parameters to apply to the RDS DB parameter group. Keys are parameter names, and values are the desired settings."
  type        = map(string)
  default     = {}
}

variable "db_sg_id" {
  description = "DB security group ID"
  type        = string
}

variable "deletion_protection" {
  description = "If the DB instance should have deletion protection enabled"
  type        = bool
  default     = true
}

variable "engine" {
  description = "RDS engine"
  type        = string
  default     = "aurora-mysql"
}

variable "engine_version" {
  description = "DB engine version"
  type        = string
  default     = "aurora-mysql8.0"
}

variable "family" {
  description = "The family of the DB parameter group"
  type        = string
}

variable "instance_count" {
  description = ""
  type        = number
  default     = 2
}

variable "instance_type" {
  description = "RDS instance type for individual environments"
  type        = string
}

variable "monitoring_interval" {
  description = "The interval, in seconds, between points when Enhanced Monitoring metrics are collected for the DB instance. To disable collecting Enhanced Monitoring metrics, specify 0."
  type        = number
  default     = 0

  validation {
    condition     = contains([0, 1, 5, 10, 15, 30, 60], var.monitoring_interval)
    error_message = "Invalid value for monitoring_interval. Valid values are: 0, 1, 5, 10, 15, 30, 60 seconds."
  }
}

variable "monitoring_role_arn" {
  description = "The ARN for the IAM role that permits RDS to send enhanced monitoring metrics to CloudWatch Logs. Required if monitoring_interval is greater than 0."
  type        = string
  default     = ""
}

variable "performance_insights_enabled" {
  description = "Enable Performance Insights"
  type        = bool
  default     = true
}

variable "private_subnet_ids" {
  description = "List of private subnet IDs"
  type        = list(string)
}

variable "publicly_accessible" {
  description = "Control if instance is publicly accessible"
  type        = bool
  default     = false
}

variable "role_terraform_arn" {
  description = "Terraform IAM role ARN"
  type        = string
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}
