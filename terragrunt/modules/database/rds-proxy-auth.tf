resource "aws_secretsmanager_secret" "fts_proxy_auth_user" {
  for_each    = local.fts_proxy_auth_users
  name        = "${local.name_prefix}-fts-rds-proxy-${each.key}-credentials"
  description = "RDS Proxy auth for FTS DB user ${each.value.username}"
  tags        = merge(var.tags, { Name = "${local.name_prefix}-fts-rds-proxy-${each.key}-credentials" })
}

resource "aws_secretsmanager_secret_version" "fts_proxy_auth_user" {
  for_each  = local.fts_proxy_auth_users
  secret_id = aws_secretsmanager_secret.fts_proxy_auth_user[each.key].id
  secret_string = jsonencode({
    username = each.value.username
    password = each.value.password
  })
}

resource "aws_secretsmanager_secret" "cfs_proxy_auth_user" {
  for_each    = local.cfs_proxy_auth_users
  name        = "${local.name_prefix}-cfs-rds-proxy-${each.key}-credentials"
  description = "RDS Proxy auth for CFS DB user ${each.value.username}"
  tags        = merge(var.tags, { Name = "${local.name_prefix}-cfs-rds-proxy-${each.key}-credentials" })
}

resource "aws_secretsmanager_secret_version" "cfs_proxy_auth_user" {
  for_each  = local.cfs_proxy_auth_users
  secret_id = aws_secretsmanager_secret.cfs_proxy_auth_user[each.key].id
  secret_string = jsonencode({
    username = each.value.username
    password = each.value.password
  })
}
