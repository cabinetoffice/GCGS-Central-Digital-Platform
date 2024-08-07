output "healthcheck_user_pool_arn" {
  value = aws_cognito_user_pool.auth.arn
}

output "healthcheck_user_pool_client_id" {
  value = aws_cognito_user_pool_client.healthcheck.id
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
