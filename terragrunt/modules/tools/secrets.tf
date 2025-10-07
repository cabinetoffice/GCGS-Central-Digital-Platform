resource "random_string" "cloud_beaver_password" {
  length  = 20
  special = true
}

resource "aws_secretsmanager_secret" "cloud_beaver_credentials" {
  name        = "${local.name_prefix}-${var.cloud_beaver_config.name}-credentials"
  description = "Cloud Beaver Credentials"
  tags        = var.tags
}

resource "aws_secretsmanager_secret_version" "cloud_beaver_credentials_version" {
  secret_id = aws_secretsmanager_secret.cloud_beaver_credentials.id
  secret_string = jsonencode({
    ADMIN_USERNAME = "cbadmin",
    ADMIN_PASSWORD = random_string.cloud_beaver_password.result,
  })
}

resource "aws_secretsmanager_secret" "cloud_beaver_data_sources" {
  name = "${local.name_prefix}-${var.cloud_beaver_config.name}-data-sources"
  tags = var.tags
}

resource "aws_secretsmanager_secret_version" "cloud_beaver_data_sources" {
  secret_id     = aws_secretsmanager_secret.cloud_beaver_data_sources.id
  secret_string = local.cloud_beaver_data_sources_json
}

data "aws_secretsmanager_secret_version" "rds_creds_sirsi" {
  secret_id = var.db_sirsi_cluster_credentials_arn
}

data "aws_secretsmanager_secret_version" "rds_creds_ev" {
  secret_id = var.db_ev_cluster_credentials_arn
}

data "aws_secretsmanager_secret_version" "rds_creds_cfs" {
  secret_id = var.db_cfs_cluster_credentials_arn
}

data "aws_secretsmanager_secret_version" "rds_creds_fts" {
  secret_id = var.db_fts_cluster_credentials_arn
}
