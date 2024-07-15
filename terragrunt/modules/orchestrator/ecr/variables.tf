variable "account_ids" {
  description = "Map of all accounts and their IDs"
  type        = map(string)
}

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

variable "service_configs" {
  description = "Map of services to their ports"
  type = map(object({
    cpu       = number
    memory    = number
    name      = string
    port      = number
    port_host = number
  }))
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}
