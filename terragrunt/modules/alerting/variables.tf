variable "ecs_alb_arn_suffix" {
  description = "ALB's ARN suffix to address target service metrics"
  type        = string
}

variable "ecs_cluster_name" {
  description = "ECS Cluster name"
  type        = string
}

variable "ecs_fts_alb_arn_suffix" {
  description = "FTS ALB ARN suffix for target metrics"
  type        = string
}

variable "ecs_fts_services_target_group_arn_suffix_map" {
  description = "FTS target groups ARN suffix mapped to the service names to address service metrics"
  type        = map(string)
}

variable "ecs_services_target_group_arn_suffix_map" {
  description = "Target groups ARN suffix mapped to the service names to address service metrics"
  type        = map(string)
}

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
