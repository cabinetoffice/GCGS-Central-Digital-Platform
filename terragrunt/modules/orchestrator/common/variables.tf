variable "pinned_service_versions" {
  description = "Specifies the pinned service versions for each environment, if defined."
  type        = map(string)
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
