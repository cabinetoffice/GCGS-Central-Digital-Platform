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

variable "create_read_replica" {
  description = "Create a read replica for this RDS instance"
  type        = bool
  default     = true
}

variable "db_name" {
  description = "Data base name"
  type        = string
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
  default     = "postgres"
}

variable "engine_version" {
  description = "DB engine version"
  type        = string
}

variable "family" {
  description = "The family of the DB parameter group"
  type        = string
}

variable "instance_type" {
  description = "RDS instance type for individual environments"
  type        = string
}

variable "max_allocated_storage" {
  description = "The maximum amount of storage (in GB) that can be automatically allocated to the RDS instance."
  type        = number
  default     = 0
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

variable "multi_az" {
  description = "Enable Multi-AZ deployment for RDS"
  type        = bool
  default     = true
}

variable "parameter_group_name" {
  description = "The name of the DB parameter group."
  type        = string
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

variable "storage_type" {
  description = "The storage type to use for RDS (gp2, gp3, io1)"
  type        = string
  default     = "gp2"
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}
