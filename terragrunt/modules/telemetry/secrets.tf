resource "aws_secretsmanager_secret" "grafana_credentials" {
  name        = "${local.name_prefix}-${var.grafana_config.name}-credentials"
  description = "Grafana Credentials"
  tags        = var.tags
}

data "aws_secretsmanager_secret" "grafana_entra_id" {
  count = var.grafana_azuread_enabled ? 1 : 0
  name  = "${local.name_prefix}-${var.grafana_config.name}-entra-id"
}
