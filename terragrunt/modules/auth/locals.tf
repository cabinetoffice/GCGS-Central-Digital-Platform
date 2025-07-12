locals {
  name_prefix = var.product.resource_name

  auth_domain             = "${local.name_prefix}-${var.environment}"

  cfs_domain              = "${local.auth_domain}-cfs"
  cfs_url                 = "https://cfs.${var.public_domain}"
  cfs_healthcheck_domain  = "${local.auth_domain}-cfs-healthcheck"
  cfs_healthcheck_url     = "https://cfs-healthcheck.${var.public_domain}"

  cloud_beaver_domain     = "${local.auth_domain}-cloud-beaver"
  cloud_beaver_url        = "https://cloud-beaver.${var.public_domain}"

  fts_domain              = "${local.auth_domain}-fts"
  fts_url                 = "https://fts.${var.public_domain}"
  fts_healthcheck_domain  = "${local.auth_domain}-fts-healthcheck"
  fts_healthcheck_url     = "https://fts-healthcheck.${var.public_domain}"

  grafana_domain          = "${local.auth_domain}-grafana"
  grafana_url             = "https://grafana.${var.public_domain}"

  healthcheck_domain      = "${local.auth_domain}-healthcheck"
  healthcheck_url         = "https://healthcheck.${var.public_domain}"

  organisation_app_domain = "${local.auth_domain}-organisatino-app"
  organisation_app_url    = "https://${var.public_domain}"

  pgadmin_domain          = "${local.auth_domain}-pgadmin"
  pgadmin_url             = "https://pgadmin.${var.public_domain}"
}
