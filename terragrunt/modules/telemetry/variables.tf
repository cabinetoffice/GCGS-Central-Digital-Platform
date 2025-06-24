variable "account_ids" {
  description = "Map of all accounts and their IDs"
  type        = map(string)
}

variable "ecs_alb_dns_name" {
  description = "DNS to the load balancer in front of ECS services"
  type        = string
}

variable "ecs_alb_sg_id" {
  description = "Application load-balancer security group ID"
  type        = string
}

variable "ecs_cluster_id" {
  description = "ECS Cluster ID"
  type        = string
}

variable "ecs_listener_arn" {
  description = "ECS Application Loadbalancer Listener ARN"
  type        = string
}

variable "ecs_sg_id" {
  description = "ECS security group ID"
  type        = string
}

variable "efs_fluentbit_access_point_id" {
  description = "The FluentBit's EFS Access Point ID"
  type        = string
}

variable "efs_fluentbit_container_path" {
  description = "The path to mount in container"
  type        = string
}

variable "efs_fluentbit_id" {
  description = "The FluentBit's EFS ID"
  type        = string
}

variable "efs_fluentbit_volume_name" {
  description = "The FluentBit's mounted volume with appropriate access"
}

variable "efs_sg_id" {
  description = "EFS security group ID"
  type        = string
}

variable "environment" {
  description = "The environment we are provisioning"
  type        = string
}

variable "fluentbit_config" {
  description = "Fluent Bit service configuration"
  type = object({
    cpu       = number
    memory    = number
    name      = string
    port      = number
    port_host = number
  })
}

variable "grafana_config" {
  description = "Grafana services configuration"
  type = object({
    cpu       = number
    memory    = number
    name      = string
    port      = number
    port_host = number
  })
}

variable "is_production" {
  description = "Indicates whether the target account is configured with production-level settings"
  type        = bool
}

variable "private_subnet_ids" {
  description = "List of private subnet IDs"
  type        = list(string)
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

variable "public_hosted_zone_id" {
  description = "ID of the public hosted zone"
  type        = string
}

variable "role_ecs_task_arn" {
  description = "Task IAM role ARN"
  type        = string
}

variable "role_ecs_task_exec_arn" {
  description = "Task execution IAM role ARN"
  type        = string
}

variable "role_ecs_task_name" {
  description = "Task IAM role Name"
  type        = string
}

variable "role_telemetry_arn" {
  description = "IAM role to be assumed by Garafana Cloudwatch datasource"
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

variable "user_pool_arn_grafana" {
  default = null
  type    = string
}

variable "user_pool_client_id_grafana" {
  default = null
  type    = string
}

variable "user_pool_domain_grafana" {
  default = null
  type    = string
}

variable "vpc_id" {
  description = "The ID of the VPC"
  type        = string
}
