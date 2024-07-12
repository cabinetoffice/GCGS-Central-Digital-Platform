variable "db_postgres_sg_id" {
  description = "Postgres DB security group ID"
  type        = string
}

variable "environment" {
  description = "The environment we are provisioning, i.e. test, do not mistake this with the AWS account"
  type        = string
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

variable "role_cloudwatch_events_arn" {
  description = "ARN of the IAM role used by CloudWatch Events"
  type        = string
}

variable "role_cloudwatch_events_name" {
  description = "Name of the IAM role used by CloudWatch Events"
  type        = string
}

variable "role_db_connection_step_function_name" {
  description = "Name of the IAM role used by the Step Function in charge of updating DB connection secret"
  type        = string
}

variable "role_db_connection_step_function_arn" {
  description = "ARN of the IAM role used by the Step Function in charge of updating DB connection secret"
  type        = string
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}
