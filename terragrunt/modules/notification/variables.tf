variable "mail_from_domain" {
  description = "Domain name to be used for sending email from in each account"
  type        = string
  default     = null
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
