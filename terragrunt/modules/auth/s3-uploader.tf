resource "aws_cognito_user_pool" "tools" {
  name = local.tools_pool_name

  admin_create_user_config {
    allow_admin_create_user_only = true
  }

  password_policy {
    minimum_length                   = 8
    require_lowercase                = true
    require_numbers                  = true
    require_symbols                  = true
    require_uppercase                = true
    temporary_password_validity_days = 1
  }

  username_configuration {
    case_sensitive = false
  }
}

resource "aws_cognito_user_pool_domain" "tools" {
  domain       = local.tools_domain
  user_pool_id = aws_cognito_user_pool.tools.id
}

resource "aws_cognito_user_pool_client" "s3_uploader" {
  name = "${local.tools_pool_name}-s3-uploader"

  allowed_oauth_flows                  = ["code"]
  allowed_oauth_flows_user_pool_client = true
  allowed_oauth_scopes                 = ["openid", "email"]
  callback_urls                        = ["${local.tools_url}/oauth2/idpresponse"]
  explicit_auth_flows = [
    "ALLOW_ADMIN_USER_PASSWORD_AUTH",
    "ALLOW_CUSTOM_AUTH",
    "ALLOW_USER_PASSWORD_AUTH",
    "ALLOW_USER_SRP_AUTH",
    "ALLOW_REFRESH_TOKEN_AUTH"
  ]
  generate_secret              = true
  logout_urls                  = ["${local.tools_url}", "${local.tools_url}/logout"]
  supported_identity_providers = ["COGNITO"]
  user_pool_id                 = aws_cognito_user_pool.tools.id
}
