resource "random_string" "grafana_admin_password" {
  length  = 20
  special = true
}

resource "aws_secretsmanager_secret" "grafana_credentials" {
  name        = "${local.name_prefix}-${var.grafana_config.name}-credentials"
  description = "Grafana Credentials"
  tags        = var.tags
}

resource "aws_secretsmanager_secret_version" "grafana_credentials_version" {
  secret_id = aws_secretsmanager_secret.grafana_credentials.id
  secret_string = jsonencode({
    ADMIN_USERNAME = "sirsi-admin",
    ADMIN_PASSWORD = random_string.grafana_admin_password.result,
  })
}
