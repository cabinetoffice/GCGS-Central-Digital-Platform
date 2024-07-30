variable "account_ids" {
  description = "Map of all accounts and their IDs"
  type        = map(string)
}

variable "db_address" {
  description = "Database endpoint address"
  type        = string
}

variable "db_credentials" {
  description = "Database secret ARN"
  type        = string
}

variable "db_kms_arn" {
  description = "ARN of the KMS used to encrypt database secrets"
  type        = string
}

variable "db_name" {
  description = "Database name"
  type        = string
}

variable "ecs_alb_sg_id" {
  description = "Application load-balancer security group ID"
  type        = string
}

variable "ecs_cluster_id" {
  description = "ECS Cluster ID"
  type        = string
}

variable "ecs_lb_dns_name" {
  description = "ECS Application Loadbalancer DNS name"
  type        = string
}

variable "ecs_listener_arn" {
  description = "ECS Application Loadbalancer Listener ARN"
  type        = string
}

variable "ecs_sg_id" {
  description = "ECS security group ID"
  type        = string
}

variable "environment" {
  description = "The environment we are provisioning"
  type        = string
}

variable "healthcheck_config" {
  description = "Health-check services configuration"
  type = object({
    cpu       = number
    memory    = number
    name      = string
    port      = number
    port_host = number
  })
}

variable "pgadmin_config" {
  description = "Pgadmin services configuration"
  type = object({
    cpu       = number
    memory    = number
    name      = string
    port      = number
    port_host = number
  })
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

variable "public_hosted_zone_fqdn" {
  description = "Fully qualified domain name of the public hosted zone"
  type        = string
}

variable "public_hosted_zone_id" {
  description = "ID of the public hosted zone"
  type        = string
}

variable "queue_entity_verification_queue_arn" {
  description = "ARN of the Entity Verification's SQS queue"
  type        = string
}

variable "queue_entity_verification_queue_url" {
  description = "URL of the Entity Verification's SQS queue"
  type        = string
}

variable "queue_organisation_queue_arn" {
  description = "ARN of the Organisation's SQS queue"
  type        = string
}

variable "queue_organisation_queue_url" {
  description = "URL of the Organisation's outbound SQS queue"
  type        = string
}

variable "role_ecs_task_arn" {
  description = "Task IAM role ARN"
  type        = string
}

variable "role_ecs_task_exec_arn" {
  description = "Task execution IAM role ARN"
  type        = string
}

variable "role_ecs_task_name" {
  description = "Task IAM role Name"
  type        = string
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}

variable "vpc_id" {
  description = "The ID of the VPC"
  type        = string
}
