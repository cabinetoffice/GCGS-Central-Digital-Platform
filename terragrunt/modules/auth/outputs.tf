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
  value = aws_cognito_user_pool.opensearch_admin.arn
}

output "opensearch_admin_user_pool_client_id" {
  value = aws_cognito_user_pool_client.opensearch_admin.id
}

output "opensearch_admin_user_pool_domain" {
  value = aws_cognito_user_pool_domain.opensearch_admin.domain
}

output "opensearch_debugtask_user_pool_arn" {
  value = aws_cognito_user_pool.opensearch_debugtask.arn
}

output "opensearch_debugtask_user_pool_client_id" {
  value = aws_cognito_user_pool_client.opensearch_debugtask.id
}

output "opensearch_debugtask_user_pool_domain" {
  value = aws_cognito_user_pool_domain.opensearch_debugtask.domain
}

output "opensearch_gateway_user_pool_arn" {
  value = aws_cognito_user_pool.opensearch_gateway.arn
}

output "opensearch_gateway_user_pool_client_id" {
  value = aws_cognito_user_pool_client.opensearch_gateway.id
}

output "opensearch_gateway_user_pool_domain" {
  value = aws_cognito_user_pool_domain.opensearch_gateway.domain
}

output "organisation_app_user_pool_arn" {
  value = aws_cognito_user_pool.auth.arn
}

output "organisation_app_user_pool_client_id" {
  value = aws_cognito_user_pool_client.organisation_app.id
}

output "tools_user_pool_arn" {
  value = aws_cognito_user_pool.tools.arn
}

output "tools_user_pool_client_id_s3_uploader" {
  value = aws_cognito_user_pool_client.s3_uploader.id
}

output "tools_user_pool_domain" {
  value = aws_cognito_user_pool_domain.tools.domain
}

output "user_management_app_user_pool_client_id" {
  value = aws_cognito_user_pool_client.user_management_app.id
}

output "user_pool_domain" {
  value = aws_cognito_user_pool_domain.auth.domain
}
