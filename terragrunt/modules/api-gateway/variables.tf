variable "role_api_gateway_cloudwatch_arn" {
  description = "IAM role ID for API Gateway to use when interacting with Cloudwatch"
  type        = string
}

variable "environment" {
  description = "The environment we are provisioning"
  type        = string
}

variable "lb_ecs_dns_name" {
  description = "DNS name of the ECS's load-balancer"
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
    cpu           = number
    memory        = number
    name          = string
    port          = number
    port_listener = number
  }))
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}
