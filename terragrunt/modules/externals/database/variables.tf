variable "db_mysql_sg_id" {
  description = "MySQL DB security group ID"
  type        = string
}

variable "externals_product" {
  description = "External product's common attributes"
  type = object({
    name                           = string
    resource_name                  = string
    mysql_access_allowed_ip_ranges = list(string)
  })
}

variable "is_production" {
  description = "Indicates whether the target account is configured with production-level settings"
  type        = bool
}


variable "private_subnet_ids" {
  description = "List of private subnet IDs"
  type        = list(string)
}

variable "role_rds_cloudwatch_arn" {
  description = "The ARN for the IAM role that permits RDS to send data to CloudWatch. Required in production accounts where enhanced monitoring is enabled"
  type        = string
  default     = ""
}

variable "role_terraform_arn" {
  description = "Terraform IAM role ARN"
  type        = string
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}
