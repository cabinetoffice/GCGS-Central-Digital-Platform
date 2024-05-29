variable "environment" {
  description = "The environment we are provisioning"
  type        = string
}

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

variable "terraform_operators" {
  description = "List of IAM user ARNs allowed to assume terraform roles"
  type        = list(string)
}

variable "tfstate_bucket_name" {
  description = "Terraform state files S3 Bucket name"
  type        = string
}
