locals {
  name_prefix        = var.product.resource_name
  teams_webhook_secret_arn = data.aws_secretsmanager_secret.teams_webhook.arn
}
