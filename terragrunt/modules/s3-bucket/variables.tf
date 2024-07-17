variable "bucket_name" {
  description = "The name of the S3 bucket"
  type        = string
}

variable "read_roles" {
  description = "A list of ARNs to allow actions for reading files"
  type        = list(string)
  default     = []
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}

variable "write_roles" {
  description = "A list of ARNs to allow actions for writing files"
  type        = list(string)
  default     = []
}
