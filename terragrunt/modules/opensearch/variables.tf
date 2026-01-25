variable "availability_zone_count" {
  description = "AZ count for zone awareness. Use 2 or 3."
  type        = number
  default     = 2
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

variable "role_opensearch_admin_arn" {
  description = "ARN the IAM role to be used to administrate OpenSearch"
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
