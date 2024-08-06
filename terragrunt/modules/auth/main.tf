resource "aws_cognito_user_pool" "auth" {
  name = local.name_prefix

  auto_verified_attributes = ["email"]
  username_attributes      = ["email"]

  password_policy {
    minimum_length    = 8
    require_lowercase = true
    require_numbers   = true
    require_symbols   = true
    require_uppercase = true
  }

  schema {
    attribute_data_type = "String"
    name                = "email"
    required            = true
    mutable             = true

    string_attribute_constraints {
      min_length = 5
      max_length = 50
    }
  }

  username_configuration {
    case_sensitive = false
  }

}

resource "aws_cognito_user_pool_domain" "auth" {
  domain       = local.auth_domain
  user_pool_id = aws_cognito_user_pool.auth.id
}