variable "elasticache_redis_sg_id" {
  description = "ElastiCache security group ID"
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

variable "engine" {
  description = "Name of the cache engine to be used for the clusters in this replication group"
  type        = string
  default     = "redis"

  validation {
    condition = contains(["redis", "valkey"], var.engine)
    error_message = "Invalid value for engine. Valid values are 'redis' or 'valkey'."
  }
}

variable "engine_version" {
  description = "Version number of the cache engine to be used for the cache clusters in this replication group"
  type        = string
  default     = "6.x"
}

variable "family" {
  description = "The family of the ElastiCache parameter group."
  type        = string
  default     = "redis6.x"
}
variable "node_type" {
  description = "Instance class to be used"
  type        = string
  default     = "cache.t3.medium"
}

# variable "parameter_group_name" {
#   description = "Name of the parameter group to associate with this replication group."
#   type        = string
#   default     = "default.redis6.x.cluster.on"
# }

variable "private_subnet_ids" {
  description = "List of private subnet IDs"
  type = list(string)
}

variable "port" {
  description = "Port number on which each of the cache nodes will accept."
  type        = number
  default     = 6379
}

variable "product" {
  description = "product's common attributes"
  type = object({
    name               = string
    resource_name      = string
    public_hosted_zone = string
  })
}

# variable "role_rds_cloudwatch_arn" {
#   description = "The ARN for the IAM role that permits RDS to send data to CloudWatch. Required in production accounts where enhanced monitoring is enabled"
#   type        = string
#   default     = ""
# }
#
# variable "role_terraform_arn" {
#   description = "Terraform IAM role ARN"
#   type        = string
# }

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type = map(string)
}
