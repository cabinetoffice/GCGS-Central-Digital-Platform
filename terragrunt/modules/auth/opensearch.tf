resource "aws_cognito_user_pool_client" "opensearch_admin" {
  name = local.opensearch_admin_domain

  allowed_oauth_flows                  = ["code"]
  allowed_oauth_flows_user_pool_client = true
  allowed_oauth_scopes                 = ["openid"]
  callback_urls                        = ["${local.opensearch_admin_url}/oauth2/idpresponse"]
  explicit_auth_flows = [
    "ALLOW_ADMIN_USER_PASSWORD_AUTH",
    "ALLOW_CUSTOM_AUTH",
    "ALLOW_USER_PASSWORD_AUTH",
    "ALLOW_USER_SRP_AUTH",
    "ALLOW_REFRESH_TOKEN_AUTH"
  ]
  generate_secret              = true
  logout_urls                  = ["${local.opensearch_admin_url}/logout"]
  supported_identity_providers = ["COGNITO"]
  user_pool_id                 = aws_cognito_user_pool.opensearch_admin.id
}

resource "aws_cognito_user_pool_client" "opensearch_gateway" {
  name = local.opensearch_gateway_domain

  allowed_oauth_flows                  = ["code"]
  allowed_oauth_flows_user_pool_client = true
  allowed_oauth_scopes                 = ["openid"]
  callback_urls                        = ["${local.opensearch_gateway_url}/oauth2/idpresponse"]
  explicit_auth_flows = [
    "ALLOW_ADMIN_USER_PASSWORD_AUTH",
    "ALLOW_CUSTOM_AUTH",
    "ALLOW_USER_PASSWORD_AUTH",
    "ALLOW_USER_SRP_AUTH",
    "ALLOW_REFRESH_TOKEN_AUTH"
  ]
  generate_secret              = true
  logout_urls                  = ["${local.opensearch_gateway_url}/logout"]
  supported_identity_providers = ["COGNITO"]
  user_pool_id                 = aws_cognito_user_pool.opensearch_gateway.id
}

resource "aws_cognito_user_pool_client" "opensearch_debugtask" {
  name = local.opensearch_debugtask_domain

  allowed_oauth_flows                  = ["code"]
  allowed_oauth_flows_user_pool_client = true
  allowed_oauth_scopes                 = ["openid"]
  callback_urls                        = ["${local.opensearch_debugtask_url}/oauth2/idpresponse"]
  explicit_auth_flows = [
    "ALLOW_ADMIN_USER_PASSWORD_AUTH",
    "ALLOW_CUSTOM_AUTH",
    "ALLOW_USER_PASSWORD_AUTH",
    "ALLOW_USER_SRP_AUTH",
    "ALLOW_REFRESH_TOKEN_AUTH"
  ]
  generate_secret              = true
  logout_urls                  = ["${local.opensearch_debugtask_url}/logout"]
  supported_identity_providers = ["COGNITO"]
  user_pool_id                 = aws_cognito_user_pool.opensearch_debugtask.id
}

resource "aws_cognito_user_pool" "opensearch_admin" {
  name = local.opensearch_admin_pool_name

  admin_create_user_config {
    allow_admin_create_user_only = true
    invite_message_template {
      email_subject = "CDP SIRSI ${var.environment} - OpenSearch Admin temporary password"
      email_message = "You have been invited to CDP SIRSI ${var.environment} - OpenSearch Admin. Username: {username} Temporary password: {####}"
      sms_message   = "CDP SIRSI ${var.environment} - OpenSearch Admin. Username: {username} Temp password: {####}"
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
    email_subject        = "CDP SIRSI ${var.environment} - OpenSearch Admin verification code"
    email_message        = "Your verification code for CDP SIRSI ${var.environment} - OpenSearch Admin is {####}"
  }

  username_configuration {
    case_sensitive = false
  }
}

resource "aws_cognito_user_pool_domain" "opensearch_admin" {
  domain       = local.opensearch_admin_pool_domain
  user_pool_id = aws_cognito_user_pool.opensearch_admin.id
}

resource "aws_cognito_user_pool" "opensearch_gateway" {
  name = local.opensearch_gateway_pool_name

  admin_create_user_config {
    allow_admin_create_user_only = true
    invite_message_template {
      email_subject = "CDP SIRSI ${var.environment} - OpenSearch Gateway temporary password"
      email_message = "You have been invited to CDP SIRSI ${var.environment} - OpenSearch Gateway. Username: {username} Temporary password: {####}"
      sms_message   = "CDP SIRSI ${var.environment} - OpenSearch Gateway. Username: {username} Temp password: {####}"
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
    email_subject        = "CDP SIRSI ${var.environment} - OpenSearch Gateway verification code"
    email_message        = "Your verification code for CDP SIRSI ${var.environment} - OpenSearch Gateway is {####}"
  }

  username_configuration {
    case_sensitive = false
  }
}

resource "aws_cognito_user_pool_domain" "opensearch_gateway" {
  domain       = local.opensearch_gateway_pool_domain
  user_pool_id = aws_cognito_user_pool.opensearch_gateway.id
}

resource "aws_cognito_user_pool" "opensearch_debugtask" {
  name = local.opensearch_debugtask_pool_name

  admin_create_user_config {
    allow_admin_create_user_only = true
    invite_message_template {
      email_subject = "CDP SIRSI ${var.environment} - OpenSearch Debug Task temporary password"
      email_message = "You have been invited to CDP SIRSI ${var.environment} - OpenSearch Debug Task. Username: {username} Temporary password: {####}"
      sms_message   = "CDP SIRSI ${var.environment} - OpenSearch Debug Task. Username: {username} Temp password: {####}"
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
    email_subject        = "CDP SIRSI ${var.environment} - OpenSearch Debug Task verification code"
    email_message        = "Your verification code for CDP SIRSI ${var.environment} - OpenSearch Debug Task is {####}"
  }

  username_configuration {
    case_sensitive = false
  }
}

resource "aws_cognito_user_pool_domain" "opensearch_debugtask" {
  domain       = local.opensearch_debugtask_pool_domain
  user_pool_id = aws_cognito_user_pool.opensearch_debugtask.id
}
