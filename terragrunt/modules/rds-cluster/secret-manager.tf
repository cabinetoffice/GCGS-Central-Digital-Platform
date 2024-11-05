resource "aws_secretsmanager_secret" "master_user_credential" {
  name        = "${var.db_name}-master-password"
  description = "The secret associated with the primary RDS cluster ${var.db_name}"
}

resource "aws_secretsmanager_secret_version" "master_user_credentials" {
  secret_id = aws_secretsmanager_secret.master_user_credential.id
  secret_string = jsonencode({
    "username" = "${replace(var.db_name, "-", "_")}_user",
    "password" = random_password.master_user_password.result
  })
}

resource "random_password" "master_user_password" {
  length  = 20
  special = true
}
