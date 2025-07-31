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
