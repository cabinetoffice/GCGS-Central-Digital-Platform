variable "bucket_name" {
  description = "The name of the S3 bucket"
  type        = string
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
