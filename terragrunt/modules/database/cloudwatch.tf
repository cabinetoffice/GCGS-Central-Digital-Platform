resource "aws_cloudwatch_log_group" "fts_db" {
  for_each = toset(local.fts_log_exports)

  name              = "/aws/rds/cluster/${local.fts_cluster_name}/${each.value}"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

