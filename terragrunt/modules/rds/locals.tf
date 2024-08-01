locals {
  db_secret   = jsondecode(data.aws_secretsmanager_secret_version.postgres.secret_string)
  db_username = local.db_secret.username
  db_password = local.db_secret.password
  db_address  = aws_db_instance.postgres.address
  db_name     = aws_db_instance.postgres.db_name
}
