variable "alb_sg_id" {
  description = "Security group ID to be attached to the ALB "
  type        = string
}

variable "canary_sg_id" {
  description = "The security group identifier for the Canary"
  type        = string
}

variable "canary_timeout_seconds" {
  description = "The time in seconds before the Canary stops executing"
  type        = number
  default     = 60
}

variable "datapoints_to_alarm" {
  description = "Number of failed datapoints"
  type        = number
  default     = 2
}

variable "environment" {
  description = "The environment in which to deploy (e.g. prod)"
  type        = string
}

variable "environment_variables" {
  description = "Map of environment variables to pass to the Canary"
  type        = map(string)
  default     = {
    API_LANDING_PAGE_URL = "https://api.dev.supplier.information.findatender.codatt.net/"
    AUTH_SECRET_NAME     = "cdp-sirsi-canary-dev-credentials"
    EXPECTED_VERSION     = "0.4.0-a5b1c239"
    WEB_DRIVER_LOG_LEVEL = "WARNING"
  }
}

variable "evaluation_periods" {
  description = "Number of periods to consider"
  type        = number
  default     = 3
}

variable "https_port" {
  description = "The port for HTTPs traffic"
  type        = number
  default     = 443
}

variable "memory_in_mb" {
  description = "The amount of memory allocated to the canary execution"
  type        = number
  default     = 1024
}

variable "period" {
  description = "Length of interval in seconds"
  type        = number
  default     = 300
}

variable "private_subnet_ids" {
  description = "The private subnets into which the Canary is deployed"
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

variable "role_canary_arn" {
  description = "Canary IAM role ARN"
  type        = string
}

variable "role_canary_name" {
  description = "Canary IAM role name"
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

variable "vpc_id" {
  description = "The identifier for the VPC"
  type        = string
}

variable "vpce_secretsmanager_sg_id" {
  description = "Security group ID of the Secrets Manager VPC endpoint"
  type        = string
}

