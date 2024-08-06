resource "aws_cognito_user_pool_client" "organisation_app" {
  name = local.organisation_app_domain

  allowed_oauth_flows                  = ["code"]
  allowed_oauth_flows_user_pool_client = true
  allowed_oauth_scopes                 = ["email", "openid"]
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

resource "aws_cognito_user_pool_ui_customization" "organisation_app" {
  client_id    = aws_cognito_user_pool_client.organisation_app.id
  image_file   = filebase64("govuk-crest.png")
  user_pool_id = aws_cognito_user_pool_domain.auth.user_pool_id
}