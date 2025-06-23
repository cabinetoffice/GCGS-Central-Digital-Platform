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

variable "role_notification_step_function_arn" {
  description = "IAM role ARN to be associated with the notification step-function"
  type        = string
}

variable "role_notification_step_function_name" {
  description = "IAM role name to be associated with the notification step-function"
  type        = string
}

variable "slack_channel_id" {
  description = "Slack channel ID to send deployment notifications too"
  type        = string
  default     = "C07FUD6GL7R"
}

variable "ssm_envs_combined_service_version_arn" {
  description = "ARN of the parameter holding all service versions for all environments"
  type        = string
}

variable "ssm_envs_combined_service_version_name" {
  description = "Name of the parameter holding all service versions for all environments"
  type        = string
}

variable "ssm_envs_fts_service_version_arn" {
  description = "ARN of the parameter holding the FTS service versions for all environments"
  type        = string
}

variable "ssm_envs_fts_service_version_name" {
  description = "Name of the parameter holding the FTS service versions for all environments"
  type        = string
}

variable "ssm_envs_sirsi_service_version_arn" {
  description = "ARN of the parameter holding the SIRSI service versions for all environments"
  type        = string
}

variable "ssm_envs_sirsi_service_version_name" {
  description = "Name of the parameter holding the SIRSI service versions for all environments"
  type        = string
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}
