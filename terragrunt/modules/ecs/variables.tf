variable "account_ids" {
  description = "Map of all accounts and their IDs"
  type        = map(string)
}

variable "alb_sg_id" {
  description = "Application load-balancer security group ID"
  type        = string
}

variable "db_entity_verification_address" {
  description = "Entity Verification DB address"
  type        = string
}

variable "db_entity_verification_credentials_arn" {
  description = "ARN of the secret holding Entity Verification DB credentials"
  type        = string
}

variable "db_entity_verification_kms_arn" {
  description = "ARN of the KMS used to encrypt Entity Verification secrets"
  type        = string
}

variable "db_entity_verification_name" {
  description = "Entity Verification DB name"
  type        = string
}

variable "db_postgres_sg_id" {
  description = "Postgres DB security group ID"
  type        = string
}

variable "db_sirsi_address" {
  description = "Sirsi DB address"
  type        = string
}

variable "db_sirsi_credentials_arn" {
  description = "ARN of the secret holding Sirsi DB credentials"
  type        = string
}

variable "db_sirsi_kms_arn" {
  description = "ARN of the KMS used to encrypt Sirsi secrets"
  type        = string
}

variable "db_sirsi_name" {
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

variable "is_production" {
  description = "Indicates whether the target account is configured with production-level settings"
  type        = bool
}

variable "pinned_service_version" {
  description = "The service version for the this environment. If null, latest version from Orchestration will be used"
  type        = string
  default     = null
}

variable "private_subnet_ids" {
  description = "List of private subnet IDs"
  type        = list(string)
}

variable "private_subnets_cidr_blocks" {
  description = "List of private subnet CIDR blocks"
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

variable "public_subnet_ids" {
  description = "List of public subnet IDs"
  type        = list(string)
}

variable "public_subnets_cidr_blocks" {
  description = "The list of public subnet CIDR blocks"
  type        = list(string)
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

variable "role_terraform_name" {
  description = "Terraform IAM role name"
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

variable "vpce_s3_sg_id" {
  description = "Security group ID of the S3 VPC endpoint"
  type        = string
}

variable "vpce_secretsmanager_sg_id" {
  description = "Security group ID of the Secrets Manager VPC endpoint"
  type        = string
}
