variable "product" {
  description = "product's common attributes"
  type = object({
    name               = string
    resource_name      = string
    public_hosted_zone = string
  })
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}

variable "teams_secret_name" {
  description = "Secrets Manager name holding Teams/Graph credentials and IDs"
  type        = string
  default     = "cdp-sirsi-teams-notification-secrets"
}

variable "lambda_timeout" {
  description = "Lambda timeout in seconds"
  type        = number
  default     = 15
}

variable "lambda_memory" {
  description = "Lambda memory size in MB"
  type        = number
  default     = 256
}

variable "dynamodb_table_name" {
  description = "Override DynamoDB table name (optional)"
  type        = string
  default     = ""
}

variable "sirsi_versions_param_name" {
  description = "SSM parameter name for SIRSI env service versions"
  type        = string
  default     = "cdp-sirsi-envs-service-version"
}

variable "fts_versions_param_name" {
  description = "SSM parameter name for FTS env service versions"
  type        = string
  default     = "cdp-sirsi-fts-envs-service-version"
}

variable "cfs_versions_param_name" {
  description = "SSM parameter name for CFS env service versions"
  type        = string
  default     = "cdp-sirsi-cfs-envs-service-version"
}
