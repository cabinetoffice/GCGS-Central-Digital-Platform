variable "audit_logs_enabled" {
  description = "Enable OpenSearch audit logs."
  type        = bool
  default     = true
}

variable "audit_logs_retention_in_days" {
  description = "Retention days for OpenSearch audit logs."
  type        = number
  default     = 30
}

variable "availability_zone_count" {
  description = "AZ count for zone awareness. Use 2 or 3."
  type        = number
  default     = 2
}

variable "dedicated_master_count" {
  description = "Number of dedicated master nodes."
  type        = number
  default     = 3
}

variable "dedicated_master_enabled" {
  description = "Enable dedicated master nodes."
  type        = bool
  default     = false
}

variable "dedicated_master_type" {
  description = "Dedicated master node instance type."
  type        = string
  default     = "t3.small.search"
}

variable "ebs_enabled" {
  description = "Enable EBS volumes for data nodes."
  type        = bool
  default     = true
}

variable "ebs_volume_size" {
  description = "EBS volume size in GiB."
  type        = number
  default     = 50
}

variable "ebs_volume_type" {
  description = "EBS volume type."
  type        = string
  default     = "gp3"
}

variable "ecs_sg_id" {
  description = "ECS security group ID"
  type        = string
}

variable "engine_version" {
  description = "OpenSearch engine version."
  type        = string
  default     = "OpenSearch_2.13"
}

variable "environment" {
  description = "The environment we are provisioning"
  type        = string
}

variable "instance_count" {
  description = "Number of data nodes."
  type        = number
  default     = 2
}

variable "instance_type" {
  description = "Data node instance type."
  type        = string
  default     = "t3.small.search"
}

variable "internal_user_database_enabled" {
  description = "Internal user database for fine grained access control (only relevant if advanced_security_enabled is true)."
  type        = bool
  default     = false
}

variable "is_production" {
  description = "Indicates whether the target account is configured with production-level settings"
  type        = bool
}

variable "opensearch_sg_id" {
  description = "OpenSearch security group ID"
  type        = string
}

variable "private_subnet_ids" {
  description = "List of private subnet IDs"
  type        = list(string)
}

variable "private_subnets_cidr_blocks" {
  description = "The CIDR block for the VPC's private subnets."
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

variable "role_ecs_task_arn" {
  description = "Task IAM role ARN"
  type        = string
}

variable "role_ecs_task_name" {
  description = "Task IAM role Name"
  type        = string
}

variable "role_ecs_task_opensearch_admin_arn" {
  description = "OpenSearch Admin Task IAM role ARN"
  type        = string
}

variable "role_ecs_task_opensearch_admin_name" {
  description = "OpenSearch Admin Task IAM role Name"
  type        = string
}

variable "role_ecs_task_opensearch_gateway_arn" {
  description = "OpenSearch Gateway Task IAM role ARN"
  type        = string
}

variable "role_ecs_task_opensearch_gateway_name" {
  description = "OpenSearch Gateway Task IAM role Name"
  type        = string
}

variable "tags" {
  description = "Tags to apply to resources."
  type        = map(string)
  default     = {}
}

variable "zone_awareness_enabled" {
  description = "Enable zone awareness (recommended if instance_count >= 2)."
  type        = bool
  default     = true
}
