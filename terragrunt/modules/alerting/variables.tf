variable "environment" {
  description = "The environment we are provisioning"
  type        = string
}

variable "product" {
  description = "product's common attributes"
  type = object({
    name               = string
    resource_name      = string
    public_hosted_zone = string
  })
}


variable "rds_cluster_ids" {
  description = "List of RDS cluster IDs"
  type        = list(string)
}

variable "redis_cluster_node_ids" {
  description = "List of ElastiCache Redis node IDs"
  type        = list(string)
}

variable "service_configs" {
  description = "Map of services to their ports"
  type = map(object({
    cpu           = number
    desired_count = number
    memory        = number
    name          = string
    port          = number
    port_host     = number
  }))
}


variable "sqs_queue_names" {
  description = "List of SQS Queue names"
  type        = list(string)
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}
