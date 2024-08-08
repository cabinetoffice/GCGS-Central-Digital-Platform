resource "aws_cognito_user_pool_client" "organisation_app" {
  name = local.organisation_app_domain

  allowed_oauth_flows                  = ["code"]
  allowed_oauth_flows_user_pool_client = true
  allowed_oauth_scopes                 = ["openid"]
  callback_urls                        = ["${local.organisation_app_url}/oauth2/idpresponse"]
  explicit_auth_flows = [
    "ALLOW_ADMIN_USER_PASSWORD_AUTH",
    "ALLOW_CUSTOM_AUTH",
    "ALLOW_USER_PASSWORD_AUTH",
    "ALLOW_USER_SRP_AUTH",
    "ALLOW_REFRESH_TOKEN_AUTH"
  ]
  generate_secret = true
  logout_urls     = ["${local.organisation_app_url}/logout"]

  supported_identity_providers = ["COGNITO"]
  user_pool_id                 = aws_cognito_user_pool.auth.id
}
