variable "db_postgres_sg_id" {
  description = "Postgres DB security group ID"
  type        = string
}

variable "environment" {
  description = "The environment we are provisioning, i.e. test, do not mistake this with the AWS account"
  type        = string
}

variable "is_production" {
  description = "Indicates whether the target account is configured with production-level settings"
  type        = bool
}

variable "postgres_engine_version" {
  description = "DB engine version"
  type        = string
}

variable "postgres_instance_type" {
  description = "RDS instance type for individual environments"
  type        = string
}

variable "private_subnet_ids" {
  description = "List of private subnet IDs"
  type        = list(string)
}

variable "product" {
  description = "product's common attributes"
  type = object({
    name               = string
    resource_name      = string
    public_hosted_zone = string
  })
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
