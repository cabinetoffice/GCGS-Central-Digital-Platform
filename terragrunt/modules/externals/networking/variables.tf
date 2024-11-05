variable "externals_product" {
  description = "External product's common attributes"
  type = object({
    name                           = string
    resource_name                  = string
    mysql_access_allowed_ip_ranges = list(string)
  })
}

variable "externals_vpc_cidr" {
  description = "The CIDR block for the VPC"
  type        = string
}

variable "externals_vpc_private_subnets" {
  description = "A list of private subnets inside the VPC"
  type        = list(string)
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}

variable "vpc_azs" {
  description = "A list of availability zones in the region"
  type        = list(string)
  default     = ["eu-west-2a", "eu-west-2b", "eu-west-2c"]
}
