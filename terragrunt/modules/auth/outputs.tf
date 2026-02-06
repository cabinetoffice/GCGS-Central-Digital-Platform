output "cfs_healthcheck_user_pool_arn" {
  value = aws_cognito_user_pool.auth.arn
}

output "cfs_healthcheck_user_pool_client_id" {
  value = aws_cognito_user_pool_client.cfs_healthcheck.id
}

output "cfs_user_pool_arn" {
  value = aws_cognito_user_pool.auth.arn
}

output "cfs_user_pool_client_id" {
  value = aws_cognito_user_pool_client.cfs.id
}

output "cloud_beaver_user_pool_arn" {
  value = aws_cognito_user_pool.auth.arn
}

output "cloud_beaver_user_pool_client_id" {
  value = aws_cognito_user_pool_client.cloud_beaver.id
}

output "commercial_tools_app_user_pool_client_id" {
  value = aws_cognito_user_pool_client.commercial_tools_app.id
}

output "fts_healthcheck_user_pool_arn" {
  value = aws_cognito_user_pool.auth.arn
}

output "fts_healthcheck_user_pool_client_id" {
  value = aws_cognito_user_pool_client.fts_healthcheck.id
}

output "fts_user_pool_arn" {
  value = aws_cognito_user_pool.auth.arn
}

output "fts_user_pool_client_id" {
  value = aws_cognito_user_pool_client.fts.id
}

output "grafana_user_pool_arn" {
  value = aws_cognito_user_pool.auth.arn
}

output "grafana_user_pool_client_id" {
  value = aws_cognito_user_pool_client.grafana.id
}

output "healthcheck_user_pool_arn" {
  value = aws_cognito_user_pool.auth.arn
}

output "healthcheck_user_pool_client_id" {
  value = aws_cognito_user_pool_client.healthcheck.id
}

output "opensearch_admin_user_pool_arn" {
  value = aws_cognito_user_pool.auth.arn
}

output "opensearch_admin_user_pool_client_id" {
  value = aws_cognito_user_pool_client.opensearch_admin.id
}

output "organisation_app_user_pool_arn" {
  value = aws_cognito_user_pool.auth.arn
}

output "organisation_app_user_pool_client_id" {
  value = aws_cognito_user_pool_client.organisation_app.id
}

output "user_pool_domain" {
  value = aws_cognito_user_pool_domain.auth.domain
}

# To be used for multiuser sessions

output "tools_user_pool_arn" {
  value = aws_cognito_user_pool.tools.arn
}

output "tools_user_pool_client_id_s3_uploader" {
  value = aws_cognito_user_pool_client.s3_uploader.id
}

output "tools_user_pool_domain" {
  value = aws_cognito_user_pool_domain.tools.domain
}
