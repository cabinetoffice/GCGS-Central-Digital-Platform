resource "aws_cloudwatch_log_group" "healthcheck" {
  name              = "/ecs/healthcheck"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}


resource "aws_cloudwatch_log_group" "pgadmin" {
  name              = "/ecs/pgadmin"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}
