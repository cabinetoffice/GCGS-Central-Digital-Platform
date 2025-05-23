variable "account_ids" {
  description = "Map of all accounts and their IDs"
  type        = map(string)
}

variable "alb_sg_id" {
  description = "Application load-balancer security group ID"
  type        = string
}

variable "db_ev_cluster_address" {
  description = "Entity Verification DB address"
  type        = string
}

variable "db_ev_cluster_credentials_arn" {
  description = "ARN of the secret holding Entity Verification DB credentials"
  type        = string
}

variable "db_ev_cluster_credentials_kms_key_id" {
  description = "Key ID of the KMS used to encrypt Entity Verification secrets"
  type        = string
}

variable "db_ev_cluster_name" {
  description = "Entity Verification DB name"
  type        = string
}

variable "db_fts_cluster_address" {
  description = "Fts DB address"
  type        = string
}

variable "db_fts_cluster_credentials_arn" {
  description = "ARN of the secret holding Fts DB credentials"
  type        = string
}

variable "db_fts_cluster_name" {
  description = "Fts DB name"
  type        = string
}

variable "db_mysql_sg_id" {
  description = "Postgres DB security group ID"
  type        = string
}

variable "db_postgres_sg_id" {
  description = "Postgres DB security group ID"
  type        = string
}

variable "db_sirsi_cluster_address" {
  description = "Sirsi DB address"
  type        = string
}

variable "db_sirsi_cluster_credentials_arn" {
  description = "ARN of the secret holding Sirsi DB credentials"
  type        = string
}

variable "db_sirsi_cluster_credentials_kms_key_id" {
  description = "Key ID of the KMS used to encrypt Sirsi secrets"
  type        = string
}

variable "db_sirsi_cluster_name" {
  description = "Sirsi DB name"
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

variable "fts_service_allowed_origins" {
  description = "A list of allowed URLs"
  type        = list(string)
}

variable "is_production" {
  description = "Indicates whether the target account is configured with production-level settings"
  type        = bool
}

variable "onelogin_logout_notification_urls" {
  description = "A list of URLs that the organisation app will call to notify other services of a logout event"
  type        = list(string)
}

variable "pinned_service_version" {
  description = "The service version for the this environment. If null, latest version from Orchestration will be used"
  type        = string
  default     = null
}

variable "private_beta_domain" {
  description = "Optional fully qualified domain name (FQDN) of private-beta domain"
  type        = string
  default     = null
}

variable "private_beta_hosted_zone_id" {
  description = "Optional Private Beta Domain's Hosted Zone ID"
  type        = string
  default     = null
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
  description = "The public fully qualified domain name (FQDN)"
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

variable "queue_av_scanner_arn" {
  description = "ARN of the AV Scanner's SQS queue"
  type        = string
}

variable "queue_av_scanner_url" {
  description = "ARN of the AV Scanner's SQS queue"
  type        = string
}

variable "queue_entity_verification_arn" {
  description = "ARN of the Entity Verification's SQS queue"
  type        = string
}

variable "queue_entity_verification_url" {
  description = "URL of the Entity Verification's SQS queue"
  type        = string
}

variable "queue_organisation_arn" {
  description = "ARN of the Organisation's SQS queue"
  type        = string
}

variable "queue_organisation_url" {
  description = "URL of the Organisation's outbound SQS queue"
  type        = string
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

variable "redis_sg_id" {
  description = "ElastiCache Redis security group ID"
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

variable "role_ecs_task_name" {
  description = "Task IAM role Name"
  type        = string
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

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}

variable "user_pool_arn" {
  type = string
}

variable "user_pool_client_id" {
  type = string
}

variable "user_pool_domain" {
  type = string
}

variable "vpc_cider" {
  description = "VPC's IPv4 CIDR"
  type        = string
}

variable "vpc_id" {
  description = "The ID of the VPC"
  type        = string
}

variable "vpce_ecr_api_sg_id" {
  description = "Security group ID of the ECR API VPC endpoint"
  type        = string
}

variable "vpce_ecr_dkr_sg_id" {
  description = "Security group ID of ECR Docker VPC endpoint"
  type        = string
}

variable "vpce_logs_sg_id" {
  description = "Security group ID of Logs VPC endpoint"
  type        = string
}

variable "vpce_s3_prefix_list_id" {
  description = "Prefix list ids or S3 VPC endpoint"
  type        = string
}

variable "vpce_secretsmanager_sg_id" {
  description = "Security group ID of the Secrets Manager VPC endpoint"
  type        = string
}

variable "waf_acl_arn" {
  description = "WAF ACL ARN to be associated with the ALB"
  type        = string
}
