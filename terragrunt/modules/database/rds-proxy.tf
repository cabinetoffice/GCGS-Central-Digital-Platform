resource "aws_db_proxy" "fts" {
  name                   = "${local.name_prefix}-fts"
  engine_family          = "MYSQL"
  role_arn               = aws_iam_role.rds_proxy.arn
  vpc_subnet_ids         = var.private_subnet_ids
  vpc_security_group_ids = [var.db_mysql_sg_id]
  require_tls            = false
  debug_logging          = var.environment == "development"

  depends_on = [
    aws_iam_role_policy.rds_proxy,
    aws_secretsmanager_secret_version.fts_proxy_auth_user
  ]

  auth {
    auth_scheme = "SECRETS"
    secret_arn  = module.cluster_fts.db_credentials_arn
    iam_auth    = "DISABLED"
  }

  # PHP FTS app/worker stack uses multiple DB users (registrar/guest/migrator).
  dynamic "auth" {
    for_each = toset(local.fts_proxy_auth_secret_arns)
    content {
      auth_scheme = "SECRETS"
      secret_arn  = auth.value
      iam_auth    = "DISABLED"
    }
  }

  tags = merge(var.tags, { Name = "${local.name_prefix}-fts" })
}

resource "aws_db_proxy_default_target_group" "fts" {
  db_proxy_name = aws_db_proxy.fts.name

  connection_pool_config {
    max_connections_percent      = 100
    max_idle_connections_percent = 50
    connection_borrow_timeout    = 120
  }
}

resource "aws_db_proxy_target" "fts" {
  db_proxy_name     = aws_db_proxy.fts.name
  target_group_name = aws_db_proxy_default_target_group.fts.name

  db_cluster_identifier = module.cluster_fts.cluster_id
}

resource "aws_db_proxy" "cfs" {
  name                   = "${local.name_prefix}-cfs"
  engine_family          = "MYSQL"
  role_arn               = aws_iam_role.rds_proxy.arn
  vpc_subnet_ids         = var.private_subnet_ids
  vpc_security_group_ids = [var.db_mysql_sg_id]
  require_tls            = false
  debug_logging          = var.environment == "development"

  depends_on = [
    aws_iam_role_policy.rds_proxy,
    aws_secretsmanager_secret_version.cfs_proxy_auth_user
  ]

  auth {
    auth_scheme = "SECRETS"
    secret_arn  = module.cluster_cfs.db_credentials_arn
    iam_auth    = "DISABLED"
  }

  dynamic "auth" {
    for_each = toset(local.cfs_proxy_auth_secret_arns)
    content {
      auth_scheme = "SECRETS"
      secret_arn  = auth.value
      iam_auth    = "DISABLED"
    }
  }

  tags = merge(var.tags, { Name = "${local.name_prefix}-cfs" })
}

resource "aws_db_proxy_default_target_group" "cfs" {
  db_proxy_name = aws_db_proxy.cfs.name

  connection_pool_config {
    max_connections_percent      = 100
    max_idle_connections_percent = 50
    connection_borrow_timeout    = 120
  }
}

resource "aws_db_proxy_target" "cfs" {
  db_proxy_name     = aws_db_proxy.cfs.name
  target_group_name = aws_db_proxy_default_target_group.cfs.name

  db_cluster_identifier = module.cluster_cfs.cluster_id
}
