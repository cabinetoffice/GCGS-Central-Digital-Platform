variable "alb_sg_id" {
  description = "Application load-balancer security group ID"
  type        = string
}

variable "db_connection_secret_arn" {
  description = "ARN of the secret holding Postgres DB connection string"
  type        = string
}

variable "db_kms_arn" {
  description = "ARN of the KMS used to encrypt database secrets"
  type        = string
}

variable "db_postgres_sg_id" {
  description = "Postgres DB security group ID"
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

variable "role_service_deployer_step_function_arn" {
  description = "ARN of the IAM role used by the Service Deployer Step Function"
  type        = string
}

variable "role_service_deployer_step_function_name" {
  description = "Name of the IAM role used by the Service Deployer Step Function"
  type        = string
}

variable "service_configs" {
  description = "Map of services to their ports"
  type = map(object({
    cpu       = number
    memory    = number
    name      = string
    port      = number
    port_host = number
  }))
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}

variable "vpc_id" {
  description = "The ID of the VPC"
  type        = string
}

variable "vpc_cider" {
  description = "VPC's IPv4 CIDR"
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
