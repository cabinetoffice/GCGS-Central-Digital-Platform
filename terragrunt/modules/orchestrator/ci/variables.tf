variable "account_ids" {
  description = "Map of all accounts and their IDs"
  type        = map(string)
}

variable "ci_build_role_arn" {
  description = "CodeBuild IAM role ARN to be assigned to the CodeBuild job"
  type        = string
}

variable "ci_build_role_name" {
  description = "IAM role Name to attach required policies"
  type        = string
}

variable "ci_pipeline_role_arn" {
  description = "IAM role ARN to be assigned to the CodePipeline job"
  type        = string
}


variable "ci_pipeline_role_name" {
  description = "IAM role Name to be assigned to the CodePipeline job"
  type        = string
}

variable "ci_role_arn" {
  description = "IAM role ARN for CI/CD"
  type        = string
}

variable "ci_role_name" {
  description = "IAM role Name for CI/CD"
  type        = string
}

variable "ci_sg_id" {
  description = "Security group ID for CI/CD"
  type        = string
}

variable "environment" {
  description = "The environment we are provisioning, i.e. test, do not mistake this with the AWS account"
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

variable "role_cloudwatch_events_arn" {
  description = "ARN of the IAM role used by CloudWatch Events"
  type        = string
}

variable "role_cloudwatch_events_name" {
  description = "Name of the IAM role used by CloudWatch Events"
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

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}

variable "tfstate_bucket_name" {
  description = "Terraform state bucket name"
  type        = string
}

variable "vpc_id" {
  type = string
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
