
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

variable "public_hosted_zone_fqdn" {
  description = "Fully qualified domain name of the public hosted zone"
  type        = string
}


variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}
