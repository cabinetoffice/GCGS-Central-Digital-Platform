variable "account_ids" {
  description = "Map of all accounts and their IDs"
  type        = map(string)
}

variable "db_entity_verification_address" {
  description = "Entity Verification database endpoint address"
  type        = string
}

variable "db_entity_verification_credentials_arn" {
  description = "Entity Verification database secret ARN"
  type        = string
}

variable "db_entity_verification_name" {
  description = "Entity Verification database name"
  type        = string
}

variable "db_postgres_sg_id" {
  description = "Postgres DB security group ID"
  type        = string
}

variable "db_sirsi_address" {
  description = "Sirsi database endpoint address"
  type        = string
}

variable "db_sirsi_credentials_arn" {
  description = "Sirsi database secret ARN"
  type        = string
}

variable "db_sirsi_name" {
  description = "Sirsi database name"
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

variable "is_production" {
  description = "Indicates whether the target account is configured with production-level settings"
  type        = bool
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

variable "postgres_engine_version" {
  description = "DB engine version"
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

variable "public_domain" {
  description = "The fully qualified domain name (FQDN) that may differ from the main delegated domain specified by 'public_hosted_zone_fqdn'. This domain represents the public-facing endpoint."
  type        = string
}

variable "public_hosted_zone_id" {
  description = "ID of the public hosted zone"
  type        = string
}

variable "queue_healthcheck_queue_url" {
  description = "URL of the Health Check's SQS queue"
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

variable "role_ecs_task_exec_name" {
  description = "Task execution IAM role name"
  type        = string
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

variable "user_pool_arn_healthcheck" {
  default = null
  type    = string
}

variable "user_pool_client_id_healthcheck" {
  default = null
  type    = string
}

variable "user_pool_domain_healthcheck" {
  default = null
  type    = string
}

variable "user_pool_arn_pgadmin" {
  default = null
  type    = string
}

variable "user_pool_client_id_pgadmin" {
  default = null
  type    = string
}

variable "user_pool_domain_pgadmin" {
  default = null
  type    = string
}

variable "vpc_id" {
  description = "The ID of the VPC"
  type        = string
}
