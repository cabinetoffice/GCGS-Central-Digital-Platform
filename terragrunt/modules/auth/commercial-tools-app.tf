resource "aws_cognito_user_pool_client" "commercial_tools_app" {
  name = local.commercial_tools_app_domain

  allowed_oauth_flows                  = ["code"]
  allowed_oauth_flows_user_pool_client = true
  allowed_oauth_scopes                 = ["openid"]
  callback_urls                        = ["${local.commercial_tools_app_url}/oauth2/idpresponse"]
  explicit_auth_flows = [
    "ALLOW_ADMIN_USER_PASSWORD_AUTH",
    "ALLOW_CUSTOM_AUTH",
    "ALLOW_USER_PASSWORD_AUTH",
    "ALLOW_USER_SRP_AUTH",
    "ALLOW_REFRESH_TOKEN_AUTH"
  ]
  generate_secret = true
  logout_urls     = ["${local.commercial_tools_app_url}/signout-callback-oidc"]

  supported_identity_providers = ["COGNITO"]
  user_pool_id                 = aws_cognito_user_pool.auth.id
}
