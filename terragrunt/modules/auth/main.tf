resource "aws_cognito_user_pool" "auth" {
  name = local.name_prefix

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

resource "aws_cognito_user_pool_domain" "auth" {
  domain       = local.auth_domain
  user_pool_id = aws_cognito_user_pool.auth.id
}
