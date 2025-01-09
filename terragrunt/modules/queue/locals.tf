locals {
  name_prefix = var.product.resource_name

  name_organisation_queue        = "${local.name_prefix}-${var.environment}-organisation"
  name_entity_verification_queue = "${local.name_prefix}-${var.environment}-entity-verification"
  name_entity_healthcheck_queue  = "${local.name_prefix}-${var.environment}-healthcheck"
  name_av_scanner_queue          = "${local.name_prefix}-${var.environment}-av-scanner"
}
