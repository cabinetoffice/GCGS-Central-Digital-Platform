variable "environment" {
  description = "The environment we are provisioning, i.e. test, do not mistake this with the AWS account"
  type        = string
}

variable "mail_from_subdomain" {
  description = "Subdomain for MAIL FROM, e.g. 'mail' to create mail.domain.com"
  type        = string
  default     = "mail"
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

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}
