resource "aws_cognito_user_pool_client" "cfs" {
  name = local.cfs_domain

  allowed_oauth_flows                  = ["code"]
  allowed_oauth_flows_user_pool_client = true
  allowed_oauth_scopes                 = ["openid"]
  callback_urls                        = local.cfs_callback_urls
  explicit_auth_flows = [
    "ALLOW_ADMIN_USER_PASSWORD_AUTH",
    "ALLOW_CUSTOM_AUTH",
    "ALLOW_USER_PASSWORD_AUTH",
    "ALLOW_USER_SRP_AUTH",
    "ALLOW_REFRESH_TOKEN_AUTH"
  ]
  generate_secret              = true
  logout_urls                  = local.cfs_logout_urls
  supported_identity_providers = ["COGNITO"]
  user_pool_id                 = aws_cognito_user_pool.auth.id
}

resource "aws_cognito_user_pool_client" "cfs_healthcheck" {
  name = local.cfs_healthcheck_domain

  allowed_oauth_flows                  = ["code"]
  allowed_oauth_flows_user_pool_client = true
  allowed_oauth_scopes                 = ["openid"]
  callback_urls                        = ["${local.cfs_healthcheck_url}/oauth2/idpresponse"]
  explicit_auth_flows = [
    "ALLOW_ADMIN_USER_PASSWORD_AUTH",
    "ALLOW_CUSTOM_AUTH",
    "ALLOW_USER_PASSWORD_AUTH",
    "ALLOW_USER_SRP_AUTH",
    "ALLOW_REFRESH_TOKEN_AUTH"
  ]
  generate_secret              = true
  logout_urls                  = ["${local.cfs_healthcheck_url}/logout"]
  supported_identity_providers = ["COGNITO"]
  user_pool_id                 = aws_cognito_user_pool.auth.id
}
