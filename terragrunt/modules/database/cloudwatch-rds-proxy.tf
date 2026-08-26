resource "aws_cloudwatch_log_group" "fts_rds_proxy" {
  name              = "/aws/rds/proxy/${local.name_prefix}-fts"
  retention_in_days = local.rds_proxy_log_retention_in_days
  tags              = merge(var.tags, { Name = "/aws/rds/proxy/${local.name_prefix}-fts" })
}

resource "aws_cloudwatch_log_group" "cfs_rds_proxy" {
  name              = "/aws/rds/proxy/${local.name_prefix}-cfs"
  retention_in_days = local.rds_proxy_log_retention_in_days
  tags              = merge(var.tags, { Name = "/aws/rds/proxy/${local.name_prefix}-cfs" })
}
