resource "random_string" "redis_auth_token" {
  length  = 20
  special = true
}

resource "aws_secretsmanager_secret" "redis_auth_token" {
  name        = "${local.name_prefix}-redis-auth-token"
  description = "Redis authentication token"
  tags        = var.tags
}

resource "aws_secretsmanager_secret_version" "redis_auth_token" {
  secret_id     = aws_secretsmanager_secret.redis_auth_token.id
  secret_string = random_string.redis_auth_token.result
}
