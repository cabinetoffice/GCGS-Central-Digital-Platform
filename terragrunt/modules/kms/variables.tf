variable "bucket_enable_presigned" {
  type        = bool
  default     = false
  description = "Allow pre-signed URLs for S3 buckets"
}

variable "bucket_name" {
  type        = string
  default     = null
  description = "Used if bucket_enable_presigned is true"
}

variable "custom_policies" {
  type = list(string)
}

variable "customer_master_key_spec" {
  type = string
}

variable "deletion_window_in_days" {
  type = number
}

variable "description" {
  type = string
}

variable "key_admin_role" {
  type = string
}

variable "key_alias" {
  type = string
}

variable "key_usage" {
  type = string
}

variable "key_user_arns" {
  type = set(string)
}

variable "other_aws_accounts" {
  type = map(string)
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}
