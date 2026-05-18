import {
  to = aws_cloudwatch_log_group.fts_db["audit"]
  id = "/aws/rds/cluster/${local.fts_cluster_name}/audit"
}

import {
  to = aws_cloudwatch_log_group.fts_db["error"]
  id = "/aws/rds/cluster/${local.fts_cluster_name}/error"
}

import {
  to = aws_cloudwatch_log_group.fts_db["general"]
  id = "/aws/rds/cluster/${local.fts_cluster_name}/general"
}

import {
  to = aws_cloudwatch_log_group.fts_db["slowquery"]
  id = "/aws/rds/cluster/${local.fts_cluster_name}/slowquery"
}

resource "aws_cloudwatch_log_group" "fts_db" {
  for_each = toset(local.fts_log_exports)

  name              = "/aws/rds/cluster/${local.fts_cluster_name}/${each.value}"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

