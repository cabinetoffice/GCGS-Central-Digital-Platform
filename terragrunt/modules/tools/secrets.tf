resource "random_string" "pgadmin_admin_password" {
  length  = 20
  special = true
}

resource "aws_secretsmanager_secret" "pgadmin_credentials" {
  name = "${local.name_prefix}-${var.pgadmin_config.name}-credentials"
  description = "PGAdmin Credentials"
  tags = var.tags
}

resource "aws_secretsmanager_secret_version" "pgadmin_credentials_version" {
  secret_id = aws_secretsmanager_secret.pgadmin_credentials.id
  secret_string = jsonencode({
    ADMIN_USERNAME = "admin@sirsi.com",
    ADMIN_PASSWORD = random_string.pgadmin_admin_password.result,
  })
}
