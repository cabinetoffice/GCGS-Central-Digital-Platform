variable "environment" {
  description = "The environment we are provisioning"
  type        = string
}

variable "mail_from_domains" {
  description = "List of domain names to verify with SES and Route53 (e.g., mail-from domains)"
  type        = list(string)
  default     = []
}

variable "product" {
  description = "product's common attributes"
  type = object({
    name               = string
    resource_name      = string
    public_hosted_zone = string
  })
}

variable "public_hosted_zone_id" {
  description = "ID of the public hosted zone"
  type        = string
}

variable "role_ses_logs_ingestor_step_function_arn" {
  description = "IAM role ARN to be associated with the SES log ingestor step-function"
  type        = string
}

variable "role_ses_cloudwatch_events_arn" {
  description = "ARN of the IAM role used by CloudWatch Events"
  type        = string
}

variable "role_ses_cloudwatch_events_name" {
  description = "Name of the IAM role used by CloudWatch Events"
  type        = string
}

variable "role_ses_logs_ingestor_step_function_name" {
  description = "IAM role name to be associated with the  SES log ingestor step-function"
  type        = string
}

variable "ses_logging_event_types" {
  description = "SES event types to capture"
  type        = list(string)
  default     = ["bounce", "complaint", "delivery", "reject", "renderingFailure"]
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}
