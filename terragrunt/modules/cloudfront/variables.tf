variable "cloudfront_acm_certificate_arn" {
  type    = string
  default = null
}

variable "cloudfront_aliases" {
  type    = list(string)
  default = []

  validation {
    condition     = var.cloudfront_custom_domain_enabled == false || length(var.cloudfront_aliases) > 0
    error_message = "cloudfront_aliases must be set when cloudfront_custom_domain_enabled is true."
  }
}

variable "cloudfront_custom_domain_enabled" {
  type    = bool
  default = false
}

variable "cloudfront_default_root_object" {
  type    = string
  default = null
}

variable "cloudfront_enabled" {
  type    = bool
  default = true
}

variable "cloudfront_log_bucket_name" {
  type    = string
  default = null
}

variable "cloudfront_logging_enabled" {
  type    = bool
  default = true
}

variable "cloudfront_manage_origin_bucket" {
  type    = bool
  default = true
}

variable "cloudfront_manage_route53" {
  type    = bool
  default = false
}

variable "cloudfront_name" {
  type        = string
  description = "Optional suffix used in CloudFront resource names (defaults to 'cf')."
  default     = "cf"
}

variable "cloudfront_origin_bucket_name" {
  type    = string
  default = null

  validation {
    condition     = var.cloudfront_manage_origin_bucket || var.cloudfront_origin_bucket_name != null
    error_message = "cloudfront_origin_bucket_name must be set when cloudfront_manage_origin_bucket is false."
  }
}

variable "cloudfront_price_class" {
  type    = string
  default = "PriceClass_100"
}

variable "cloudfront_realtime_log_name_suffix" {
  type    = string
  default = null
}

variable "cloudfront_realtime_logs_enabled" {
  type    = bool
  default = true
}

variable "cloudfront_realtime_logs_fields" {
  type = list(string)
  default = [
    "timestamp",
    "c-ip",
    "time-to-first-byte",
    "sc-status",
    "sc-bytes",
    "cs-method",
    "cs-host",
    "cs-uri-stem",
    "cs-uri-query",
    "cs-user-agent",
    "cs-referer",
    "cs-protocol",
    "x-edge-location",
    "x-edge-request-id",
    "x-host-header",
  ]
}

variable "cloudfront_realtime_logs_role_arn" {
  type    = string
  default = null

  validation {
    condition     = var.cloudfront_realtime_logs_enabled == false || var.cloudfront_realtime_logs_role_arn != null
    error_message = "cloudfront_realtime_logs_role_arn must be set when cloudfront_realtime_logs_enabled is true."
  }
}

variable "cloudfront_realtime_logs_sampling_rate" {
  type    = number
  default = 100
}

variable "cloudfront_response_headers_policy_enabled" {
  type    = bool
  default = true
}

variable "cloudfront_route53_zone_id" {
  type    = string
  default = null

  validation {
    condition     = var.cloudfront_custom_domain_enabled == false || var.cloudfront_acm_certificate_arn != null || var.cloudfront_route53_zone_id != null
    error_message = "cloudfront_route53_zone_id must be set when managing ACM validation for a custom domain."
  }
}

variable "cloudfront_seed_origin" {
  type    = bool
  default = true
}

variable "environment" {
  type = string
}

variable "product" {
  type = object({
    name               = string
    resource_name      = string
    public_hosted_zone = string
  })
}

variable "tags" {
  type    = map(string)
  default = {}
}

variable "waf_bot_control_enabled" {
  type    = bool
  default = true
}

variable "waf_enabled" {
  type    = bool
  default = true
}

variable "waf_logging_enabled" {
  type    = bool
  default = true
}
