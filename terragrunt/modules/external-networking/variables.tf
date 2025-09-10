variable "core_hosted_zone_id" {
  description = "ID of the hosted zone to deploy to"
  type        = string
}

variable "environment" {
  description = "The environment we are provisioning"
  type        = string
}

variable "fts_azure_frontdoor" {
  description = "The DNS hostname to used for routing traffic to the Find a Tender Service (FTS)"
  type        = string
  default     = null
}

variable "is_production" {
  description = "Indicates whether the target account is configured with production-level settings"
  type        = bool
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
