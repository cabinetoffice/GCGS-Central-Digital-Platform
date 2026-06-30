locals {
  pcr2015_domain      = "${local.auth_domain}-pcr2015"
  pcr2015_pool_domain = "${local.auth_domain}-pcr2015"
  pcr2015_pool_name   = "${local.name_prefix}-pcr2015"
}

resource "aws_cognito_user_pool" "pcr2015" {
  name = local.pcr2015_pool_name

  admin_create_user_config {
    allow_admin_create_user_only = true
    invite_message_template {
      email_subject = "CDP SIRSI ${var.environment} - PCR 2015 temporary password"
      email_message = "You have been invited to CDP SIRSI ${var.environment} - PCR 2015. Username: {username} Temporary password: {####}"
      sms_message   = "CDP SIRSI ${var.environment} - PCR 2015. Username: {username} Temp password: {####}"
    }
  }

  password_policy {
    minimum_length                   = 8
    require_lowercase                = true
    require_numbers                  = true
    require_symbols                  = true
    require_uppercase                = true
    temporary_password_validity_days = 1
  }

  verification_message_template {
    default_email_option = "CONFIRM_WITH_CODE"
    email_subject        = "CDP SIRSI ${var.environment} - PCR 2015 verification code"
    email_message        = "Your verification code for CDP SIRSI ${var.environment} - PCR 2015 is {####}"
  }

  username_configuration {
    case_sensitive = false
  }
}

resource "aws_cognito_user_pool_domain" "pcr2015" {
  domain       = local.pcr2015_pool_domain
  user_pool_id = aws_cognito_user_pool.pcr2015.id
}

resource "aws_cognito_user_pool_client" "pcr2015" {
  name = local.pcr2015_domain

  allowed_oauth_flows                  = ["code"]
  allowed_oauth_flows_user_pool_client = true
  allowed_oauth_scopes                 = ["openid"]
  callback_urls                        = local.fts_callback_urls
  explicit_auth_flows = [
    "ALLOW_ADMIN_USER_PASSWORD_AUTH",
    "ALLOW_CUSTOM_AUTH",
    "ALLOW_USER_PASSWORD_AUTH",
    "ALLOW_USER_SRP_AUTH",
    "ALLOW_REFRESH_TOKEN_AUTH"
  ]
  generate_secret              = true
  logout_urls                  = local.fts_logout_urls
  supported_identity_providers = ["COGNITO"]
  user_pool_id                 = aws_cognito_user_pool.pcr2015.id
}

resource "random_password" "pcr2015" {
  length           = 16
  min_lower        = 1
  min_numeric      = 1
  min_special      = 1
  min_upper        = 1
  override_special = "@#%^&*()_+|~={}[]:;<>,."
  special          = true
}

resource "aws_cognito_user" "pcr2015" {
  user_pool_id   = aws_cognito_user_pool.auth.id
  username       = "pcr2015"
  password       = random_password.pcr2015.result
  message_action = "SUPPRESS"
}

resource "aws_secretsmanager_secret" "pcr2015_credentials" {
  name        = "${local.name_prefix}-cognito/users/pcr2015"
  description = "Cognito credentials for PCR 2015 FTS access"
  tags        = var.tags
}

resource "aws_secretsmanager_secret_version" "pcr2015_credentials" {
  secret_id = aws_secretsmanager_secret.pcr2015_credentials.id
  secret_string = jsonencode({
    username = aws_cognito_user.pcr2015.username
    password = random_password.pcr2015.result
  })
}
