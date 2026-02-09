variable "bucket_name" {
  description = "The name of the S3 bucket"
  type        = string
}

variable "cors_rules" {
  description = "Optional CORS rules for the bucket"
  type        = list(any)
  default     = []
}

variable "enable_access_logging" {
  type        = bool
  default     = false
  description = "Enable server access logging for the bucket"
}

variable "enable_encryption" {
  description = "Enable server-side encryption on the bucket"
  type        = bool
  default     = true
}

variable "enable_lifecycle" {
  type        = bool
  default     = false
  description = "Enable lifecycle rule to expire files after a few days"
}

variable "enable_presigned_urls" {
  type        = bool
  default     = false
  description = "Allow pre-signed URLs for S3 buckets"
}

variable "is_public" {
  description = "Whether the bucket should be publicly accessible (e.g. for static website hosting)"
  type        = bool
  default     = false
}

variable "kms_key_admin_role" {
  default     = "bootstrap"
  description = "IAM role name to administrate the key"
  type        = string
}

variable "kms_key_description" {
  default     = ""
  description = "The description of the KMS used to encrypt S3 bucket contents"
  type        = string
}

variable "lifecycle_expiration_days" {
  type        = number
  default     = 7
  description = "Number of days before files expire (if lifecycle enabled)"
}

variable "read_roles" {
  default     = []
  description = "A list of ARNs to allow actions for reading files"
  type        = list(string)
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}

variable "write_roles" {
  default     = []
  description = "A list of ARNs to allow actions for writing files"
  type        = list(string)
}
