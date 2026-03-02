variable "allowed_github_branches" {
  description = "Allowed GitHub branches for OIDC role assumption (e.g. main, sandbox, previews/*)."
  type        = list(string)
  default     = []
}

variable "allow_github_pull_requests" {
  description = "Whether to allow GitHub pull request workflows to assume the OIDC role."
  type        = bool
  default     = false
}

variable "bucket_name" {
  description = "Optional override for the documentation bucket name."
  type        = string
  default     = null
}

variable "cloudfront_acm_certificate_arn" {
  description = "Optional ACM certificate ARN (us-east-1) for CloudFront custom domains."
  type        = string
  default     = null

  validation {
    condition     = var.cloudfront_custom_domain_enabled == false || var.cloudfront_acm_certificate_arn != null
    error_message = "cloudfront_acm_certificate_arn must be set when cloudfront_custom_domain_enabled is true."
  }
}

variable "cloudfront_aliases" {
  description = "Optional custom domain aliases for CloudFront."
  type        = list(string)
  default     = []
}

variable "cloudfront_custom_domain_enabled" {
  description = "Whether to enable a custom domain on CloudFront."
  type        = bool
  default     = false
}

variable "cloudfront_enabled" {
  description = "Whether to provision a CloudFront distribution."
  type        = bool
  default     = true
}

variable "cloudfront_price_class" {
  description = "CloudFront price class."
  type        = string
  default     = "PriceClass_100"
}

variable "docs_domain_name" {
  description = "Custom docs domain (e.g. docs.example.com)."
  type        = string
  default     = null
}

variable "enable_access_logging" {
  description = "Enable server access logging for the bucket."
  type        = bool
  default     = false
}

variable "enable_encryption" {
  description = "Enable server-side encryption on the bucket."
  type        = bool
  default     = true
}

variable "github_oidc_thumbprints" {
  description = "Thumbprints for the GitHub OIDC provider."
  type        = list(string)
  default     = ["6938fd4d98bab03faadb97b34396831e3780aea1"]
}

variable "github_org" {
  description = "GitHub organization for OIDC role trust."
  type        = string
}

variable "github_repo" {
  description = "GitHub repository for OIDC role trust."
  type        = string
}

variable "is_public" {
  description = "Whether the bucket should be publicly accessible."
  type        = bool
  default     = true
}

variable "product" {
  description = "Product attributes used for naming."
  type = object({
    name               = string
    resource_name      = string
    public_hosted_zone = string
  })
}

variable "sse_algorithm" {
  description = "Server-side encryption algorithm for the bucket."
  type        = string
  default     = "AES256"
}

variable "tags" {
  description = "Tags to apply to resources."
  type        = map(string)
}
