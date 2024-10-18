variable "add_security_group_roles" {
  default     = true
  description = "Whether attache SG rules to enable communication between the LB and ECS service. "
  type        = bool
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

variable "family" {
  description = "A unique name for the task definition"
  type        = string
}

variable "healthcheck_enabled" {
  description = "Health-check state"
  default     = true
}

variable "healthcheck_healthy_threshold" {
  description = "Health-check threshold"
  default     = 3
}

variable "healthcheck_interval" {
  description = "Health-check interval"
  default     = 60
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
