variable "pen_testing_user_arns" {
  description = "List of user ARNs to grant access for pen testing"
  type        = list(string)
  default     = []
}

variable "private_route_table_ids" {
  type = list(string)
}

variable "private_subnet_ids" {
  description = "List of private subnet IDs"
  type        = list(string)
}

variable "product" {
  description = "product's common attributes"
  type = object({
    name          = string
    resource_name = string
  })
}

variable "public_route_table_ids" {
  type = list(string)
}

variable "public_subnet_ids" {
  description = "List of public subnet IDs"
  type        = list(string)
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}

variable "vpc_id" {
  type = string
}

variable "vpce_ecr_api_sg_id" {
  description = "Security group ID of ECR API VPC endpoint"
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

variable "vpce_policy" {
  default = ""
  type    = string
}

variable "vpce_s3_sg_id" {
  description = "Security group ID of S3 VPC endpoint"
  type        = string
}

variable "vpce_secretsmanager_sg_id" {
  description = "Security group ID of SecretManager VPC endpoint"
  type        = string
}
