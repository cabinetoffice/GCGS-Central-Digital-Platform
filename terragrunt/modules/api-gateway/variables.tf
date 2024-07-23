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

variable "public_hosted_zone_fqdn" {
  description = "Fully qualified domain name of the public hosted zone"
  type        = string
}

variable "public_hosted_zone_id" {
  description = "ID of the public hosted zone"
  type        = string
}

variable "role_api_gateway_cloudwatch_arn" {
  description = "IAM role ID for API Gateway to use when interacting with Cloudwatch"
  type        = string
}

variable "role_api_gateway_deployer_step_function_arn" {
  description = "ARN of the IAM role used by the API Gateway Deployer Step Function"
  type        = string
}

variable "role_api_gateway_deployer_step_function_name" {
  description = "Name of the IAM role used by the API Gateway Deployer Step Function"
  type        = string
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
