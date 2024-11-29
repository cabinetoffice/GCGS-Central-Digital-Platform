variable "elasticache_redis_sg_id" {
  description = "ElastiCache security group ID"
  type        = string
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

variable "environment" {
  description = "The environment we are provisioning, i.e. test, do not mistake this with the AWS account"
  type        = string
}

variable "family" {
  description = "The family of the ElastiCache parameter group."
  type        = string
  default     = "redis6.x"
}

variable "is_production" {
  description = "Indicates whether the target account is configured with production-level settings"
  type        = bool
}

variable "node_type" {
  description = "Instance class to be used"
  type        = string
  default     = "cache.t3.medium"
}

variable "port" {
  description = "Port number on which each of the cache nodes will accept."
  type        = number
  default     = 6379
}

variable "private_subnet_ids" {
  description = "List of private subnet IDs"
  type = list(string)
}

variable "product" {
  description = "product's common attributes"
  type = object({
    name               = string
    resource_name      = string
    public_hosted_zone = string
  })
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type = map(string)
}
