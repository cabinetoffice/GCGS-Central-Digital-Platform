variable "allowed_unauthenticated_paths" {
  description = "List of paths allowed access to protected services, bypassing Cognito authentication."
  type        = list(string)
  default     = []
}

variable "cluster_id" {
  description = "Cluster ID of which the service will be part of"
  type        = string
}

variable "container_definitions" {
  description = "A list of valid container definitions provided as a single valid JSON document"
  type        = string
}

variable "container_port" {
  description = "The port number on the container that's bound to the host port"
  type        = number
}

variable "cpu" {
  description = "Number of cpu units used by the task"
  type        = number
}

variable "deployment_maximum_percent" {
  description = "Upper limit (as a percentage of the service's desiredCount) of the number of running tasks that can be running in a service during a deployment"
  default     = 200
  type        = number
}

variable "deployment_minimum_healthy_percent" {
  description = "Lower limit (as a percentage of the service's desiredCount) of the number of running tasks that must remain running and healthy in a service during a deployment"
  default     = 100
  type        = number
}

variable "desired_count" {
  description = "Number of instances of the task definition."
  default     = 1
  type        = number
}

variable "ecs_alb_sg_id" {
  description = "Application load-balancer security group ID"
  type        = string
}

variable "ecs_listener_arn" {
  description = "ECS Application Loadbalancer Listener ARN"
  type        = string
}

variable "ecs_service_base_sg_id" {
  description = "Security group ID of Flask Healthcheck ECS Service"
  type        = string
}

variable "efs_volume" {
  description = "Optional EFS volume to mount in the task definition"
  type = object({
    access_point_id    = optional(string)
    container_path     = string
    file_system_id     = string
    iam                = optional(string, "DISABLED")
    name               = string
    transit_encryption = optional(string, "ENABLED")
  })
  default = null
}

variable "extra_host_headers" {
  description = "Optional list of additional host headers to be added for this service"
  type        = list(string)
  default     = []
}

variable "family" {
  description = "A unique name for the task definition"
  type        = string
}

variable "healthcheck_healthy_threshold" {
  description = "Health-check threshold"
  default     = 3
}

variable "healthcheck_interval" {
  description = "Health-check interval"
  default     = 60
}

variable "healthcheck_matcher" {
  description = "The HTTP or gRPC codes to use when checking for a successful response from a target"
  default     = "200"
}

variable "healthcheck_path" {
  description = "Health-check path on the service"
  default     = "/health"
}

variable "healthcheck_timeout" {
  description = "Health-check timeout"
  default     = 6
}

variable "host_port" {
  description = "The port number on the container instance to reserve for container, if not set we ignore port mapping and listener links"
  type        = number
  default     = null
}

variable "is_frontend_app" {
  description = "Whether it is an API or the Frontend service, to link the domain alias to"
  type        = bool
  default     = false
}

variable "is_standalone_task" {
  description = "Whether it require a service or its standalone Task"
  type        = bool
  default     = false
}

variable "listener_name" {
  description = "Optional custom listener name if the service name exceeds the 32-character limit. The name will be prefixed with 'cdp-'."
  type        = string
  default     = null
}

variable "memory" {
  description = "Amount (in MiB) of memory used by the tas"
  type        = number
}

variable "name" {
  description = "The service name"
  type        = string
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

variable "role_ecs_task_arn" {
  description = "Task IAM role ARN"
  type        = string
}

variable "role_ecs_task_exec_arn" {
  description = "Task execution IAM role ARN"
  type        = string
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}

variable "unhealthy_threshold" {
  description = "Number of consecutive health check failures required before considering a target unhealthy. The range is 2-10. Defaults to 3."
  type        = number
  default     = 3
}

variable "user_pool_arn" {
  default = null
  type    = string
}

variable "user_pool_client_id" {
  default = null
  type    = string
}

variable "user_pool_domain" {
  default = null
  type    = string
}

variable "vpc_id" {
  description = "The ID of the VPC"
  type        = string
}
