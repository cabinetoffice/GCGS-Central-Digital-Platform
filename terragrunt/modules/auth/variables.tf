variable "cfs_extra_domains" {
  description = "Optional list of additional host headers to be added for CFS service"
  type        = list(string)
  default     = []
}

variable "environment" {
  description = "The environment we are provisioning"
  type        = string
}

variable "fts_extra_domains" {
  description = "Optional list of additional host headers to be added for FTS service"
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

variable "public_domain" {
  description = "The fully qualified domain name (FQDN) that may differ from the main delegated domain specified by 'public_hosted_zone_fqdn'. This domain represents the public-facing endpoint."
  type        = string
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}
