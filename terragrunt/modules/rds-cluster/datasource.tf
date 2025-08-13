data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_secretsmanager_secret_version" "fetched_password" {
  secret_id = aws_secretsmanager_secret.master_user_credential.id
  depends_on = [
    aws_secretsmanager_secret.master_user_credential,
    aws_secretsmanager_secret_version.master_user_credentials
  ]
}
