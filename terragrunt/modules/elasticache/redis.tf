resource "aws_elasticache_replication_group" "this" {
  at_rest_encryption_enabled  = true
  apply_immediately           = true
  auth_token                  = aws_secretsmanager_secret_version.redis_auth_token.secret_string
  auth_token_update_strategy  = "ROTATE"
  automatic_failover_enabled  = true
  description                 = "Redis cluster for organisation-app's authentication sessions"
  engine                      = var.engine
  engine_version              = var.engine_version
  multi_az_enabled            = true
  node_type                   = "cache.t3.medium"
  num_cache_clusters          = 3
  parameter_group_name        = aws_elasticache_parameter_group.this.name
  port                        = var.port
  preferred_cache_cluster_azs = data.aws_availability_zones.current.names
  replication_group_id        = "${local.name_prefix}-org-app-sessions"
  security_group_ids = [var.elasticache_redis_sg_id]
  subnet_group_name           = aws_elasticache_subnet_group.this.name
  tags                        = var.tags
  transit_encryption_enabled  = true

  log_delivery_configuration {
    destination      = aws_cloudwatch_log_group.slow_log.name
    destination_type = "cloudwatch-logs"
    log_format       = "json"
    log_type         = "slow-log"
  }

  log_delivery_configuration {
    destination      = aws_cloudwatch_log_group.engine_log.name
    destination_type = "cloudwatch-logs"
    log_format       = "json"
    log_type         = "engine-log"
  }

  depends_on = [
    aws_cloudwatch_log_resource_policy.elasticache_logs,
  ]
}

resource "aws_elasticache_parameter_group" "this" {
  description = "Used by ${local.name_prefix}, based on default ${var.family}"
  family      = var.family
  name        = "${local.name_prefix}-org-app-sessions"
  tags        = var.tags
}

resource "aws_elasticache_subnet_group" "this" {
  name       = "${local.name_prefix}-private-subnet-group"
  subnet_ids = var.private_subnet_ids

  tags = var.tags
}
