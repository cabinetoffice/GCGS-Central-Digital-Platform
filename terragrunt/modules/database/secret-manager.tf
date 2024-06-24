locals {
  db_secret   = jsondecode(data.aws_secretsmanager_secret_version.postgres.secret_string)
  db_username = local.db_secret.username
  db_password = local.db_secret.password
  db_address  = aws_db_instance.postgres.address
  db_name     = aws_db_instance.postgres.db_name
}

data "aws_secretsmanager_secret" "postgres" {
  arn = aws_db_instance.postgres.master_user_secret[0].secret_arn
}

data "aws_secretsmanager_secret_version" "postgres" {
  secret_id = data.aws_secretsmanager_secret.postgres.id
}

resource "aws_secretsmanager_secret" "db_connection_string" {
  description = "Holding Postgres DB connection string"
  name_prefix = "${local.name_prefix}-db-connection-string-"
  kms_key_id  = aws_kms_key.rds.key_id

  tags = var.tags
}

resource "aws_secretsmanager_secret_version" "db_connection_string" {
  secret_id     = aws_secretsmanager_secret.db_connection_string.id
  secret_string = "Server=${local.db_address};Database=${local.db_name};Username=${local.db_username};Password=${local.db_password}"

  depends_on = [
    data.aws_secretsmanager_secret.postgres
  ]
}
