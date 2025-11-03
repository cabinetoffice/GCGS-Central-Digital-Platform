variable "account_ids" {
  description = "Map of all accounts and their IDs"
  type        = map(string)
}

variable "alb_tools_sg_id" {
  description = "Tools application load-balancer security group ID"
  type        = string
}

variable "certificate_arn" {
  description = "ARN of the ACM certificate to use for the Tools ALB"
  type        = string
}

variable "cloud_beaver_config" {
  description = "Cloud Beaver services configuration"
  type = object({
    cpu       = number
    memory    = number
    name      = string
    port      = number
    port_host = number
  })
}

variable "db_cfs_cluster_address" {
  description = "CFS database endpoint address"
  type        = string
}

variable "db_cfs_cluster_credentials_arn" {
  description = "CFS database secret ARN"
  type        = string
}

variable "db_cfs_cluster_name" {
  description = "CFS database name"
  type        = string
}

variable "db_cfs_cluster_port" {
  description = "CFS database port"
  type        = number
  default     = 3306
}

variable "db_entity_verification_cluster_port" {
  description = "Entity Verification database port"
  type        = number
  default     = 5432
}

variable "db_ev_cluster_address" {
  description = "Entity Verification database endpoint address"
  type        = string
}

variable "db_ev_cluster_credentials_arn" {
  description = "Entity Verification database secret ARN"
  type        = string
}

variable "db_ev_cluster_name" {
  description = "Entity Verification database name"
  type        = string
}

variable "db_fts_cluster_address" {
  description = "FTS database endpoint address"
  type        = string
}

variable "db_fts_cluster_credentials_arn" {
  description = "FTS database secret ARN"
  type        = string
}

variable "db_fts_cluster_name" {
  description = "FTS database name"
  type        = string
}

variable "db_fts_cluster_port" {
  description = "FTS database port"
  type        = number
  default     = 3306
}

variable "db_postgres_sg_id" {
  description = "Postgres DB security group ID"
  type        = string
}

variable "db_sirsi_cluster_address" {
  description = "Sirsi database endpoint address"
  type        = string
}

variable "db_sirsi_cluster_credentials_arn" {
  description = "Sirsi database secret ARN"
  type        = string
}

variable "db_sirsi_cluster_name" {
  description = "Sirsi database name"
  type        = string
}

variable "db_sirsi_cluster_port" {
  description = "Sirsi database port"
  type        = number
  default     = 5432
}

variable "ecs_alb_dns_name" {
  description = "DNS to the load balancer in front of ECS services"
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

variable "ecs_cluster_name" {
  description = "ECS Cluster Name"
  type        = string
}

variable "ecs_sg_id" {
  description = "ECS security group ID"
  type        = string
}

variable "efs_sg_id" {
  description = "EFS security group ID"
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

variable "public_subnet_ids" {
  description = "List of public subnet IDs"
  type        = list(string)
}

variable "redis_auth_token_arn" {
  description = "The ARN of the Secrets Manager secret storing the Redis authentication token."
  type        = string
}

variable "redis_port" {
  description = "The port number used to connect to the ElastiCache Redis cluster."
  type        = number
}

variable "redis_primary_endpoint" {
  description = "The primary endpoint address of the ElastiCache Redis replication group."
  type        = string
}

variable "role_cloudwatch_events_arn" {
  description = "ARN of the IAM role used by CloudWatch Events"
  type        = string
}

variable "role_cloudwatch_events_name" {
  description = "Name of the IAM role used by CloudWatch Events"
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

variable "role_service_deployer_step_function_arn" {
  description = "ARN of the IAM role used by the Service Deployer Step Function"
  type        = string
}

variable "role_service_deployer_step_function_name" {
  description = "Name of the IAM role used by the Service Deployer Step Function"
  type        = string
}

variable "role_terraform_arn" {
  description = "Terraform IAM role ARN"
  type        = string
}

variable "sqs_entity_verification_url" {
  description = "URL of the Entity Verification's SQS queue"
  type        = string
}

variable "sqs_organisation_url" {
  description = "URL of the Organisation's SQS queue"
  type        = string
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}

variable "tools_configs" {
  description = "Map of tools and their attributes"
  type = map(object({
    cpu       = number
    memory    = number
    name      = string
    port      = number
    port_host = number
  }))
}

variable "user_pool_arn_cloud_beaver" {
  default = null
  type    = string
}

variable "user_pool_arn_healthcheck" {
  default = null
  type    = string
}

variable "user_pool_client_id_cloud_beaver" {
  default = null
  type    = string
}

variable "user_pool_client_id_healthcheck" {
  default = null
  type    = string
}

variable "user_pool_domain_cloud_beaver" {
  default = null
  type    = string
}

variable "user_pool_domain_healthcheck" {
  default = null
  type    = string
}

variable "vpc_id" {
  description = "The ID of the VPC"
  type        = string
}

variable "waf_acl_tools_arn" {
  description = "WAF ACL ARN to be associated with the Tools ALB"
  type        = string
}
