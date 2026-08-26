resource "aws_cloudwatch_log_group" "fts_rds_proxy" {
  name              = "/aws/rds/proxy/${aws_db_proxy.fts.name}"
  retention_in_days = local.rds_proxy_log_retention_in_days
  tags              = var.tags
}

resource "aws_cloudwatch_log_group" "cfs_rds_proxy" {
  name              = "/aws/rds/proxy/${aws_db_proxy.cfs.name}"
  retention_in_days = local.rds_proxy_log_retention_in_days
  tags              = var.tags
}

