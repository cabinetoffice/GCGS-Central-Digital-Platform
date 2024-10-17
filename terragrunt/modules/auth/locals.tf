locals {
  name_prefix             = var.product.resource_name
  auth_domain             = "${local.name_prefix}-${var.environment}"
  organisation_app_domain = "${local.name_prefix}-${var.environment}-organisatino-app"
  organisation_app_url    = "https://${var.public_domain}"
  healthcheck_domain      = "${local.name_prefix}-${var.environment}-healthcheck"
  healthcheck_url         = "https://healthcheck.${var.public_domain}"
}
