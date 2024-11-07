data "aws_secretsmanager_secret" "this" {
  arn = aws_db_instance.this.master_user_secret[0].secret_arn
}

data "aws_secretsmanager_secret_version" "this" {
  secret_id = data.aws_secretsmanager_secret.this.id
}
