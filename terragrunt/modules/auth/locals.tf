locals {
  name_prefix = var.product.resource_name

  auth_domain             = "${local.name_prefix}-${var.environment}"
  cloud_beaver_domain     = "${local.name_prefix}-${var.environment}-cloud-beaver"
  cloud_beaver_url        = "https://cloud-beaver.${var.public_domain}"
  organisation_app_domain = "${local.name_prefix}-${var.environment}-organisatino-app"
  organisation_app_url    = "https://${var.public_domain}"
  grafana_domain          = "${local.name_prefix}-${var.environment}-grafana"
  grafana_url             = "https://grafana.${var.public_domain}"
  healthcheck_domain      = "${local.name_prefix}-${var.environment}-healthcheck"
  healthcheck_url         = "https://healthcheck.${var.public_domain}"
  pgadmin_domain          = "${local.name_prefix}-${var.environment}-pgadmin"
  pgadmin_url             = "https://pgadmin.${var.public_domain}"
}
