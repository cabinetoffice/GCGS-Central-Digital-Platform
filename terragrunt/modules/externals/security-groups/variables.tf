variable "externals_product" {
  description = "External product's common attributes"
  type = object({
    name               = string
    resource_name      = string
  })
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}

variable "vpc_id" {
  type = string
}
