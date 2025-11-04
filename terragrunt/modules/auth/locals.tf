locals {
  name_prefix = var.product.resource_name

  auth_domain = "${local.name_prefix}-${var.environment}"

  cfs_domain = "${local.auth_domain}-cfs"
  cfs_url    = "cfs.${var.public_domain}"
  cfs_urls = concat(
    [local.cfs_url],
    var.cfs_extra_domains
  )
  cfs_callback_urls = [for url in local.cfs_urls : "https://${url}/oauth2/idpresponse"]
  cfs_logout_urls   = [for url in local.cfs_urls : "https://${url}/logout"]

  cfs_healthcheck_domain = "${local.auth_domain}-cfs-healthcheck"
  cfs_healthcheck_url    = "https://cfs-healthcheck.${var.public_domain}"

  cloud_beaver_domain = "${local.auth_domain}-cloud-beaver"
  cloud_beaver_url    = "https://cloud-beaver.${var.public_domain}"

  fts_domain = "${local.auth_domain}-fts"
  fts_url    = "fts.${var.public_domain}"
  fts_urls = concat(
    [local.fts_url],
    var.fts_extra_domains
  )
  fts_callback_urls = [for url in local.fts_urls : "https://${url}/oauth2/idpresponse"]
  fts_logout_urls   = [for url in local.fts_urls : "https://${url}/logout"]

  fts_healthcheck_domain = "${local.auth_domain}-fts-healthcheck"
  fts_healthcheck_url    = "https://fts-healthcheck.${var.public_domain}"

  grafana_domain = "${local.auth_domain}-grafana"
  grafana_url    = "https://grafana.${var.public_domain}"

  healthcheck_domain = "${local.auth_domain}-healthcheck"
  healthcheck_url    = "https://healthcheck.${var.public_domain}"

  organisation_app_domain = "${local.auth_domain}-organisatino-app"
  organisation_app_url    = "https://${var.public_domain}"

  commercial_tools_app_domain = "${local.auth_domain}-commercial_tools_app"
  commercial_tools_app_url    = "https://commercial-tools-app.${var.public_domain}"
}
