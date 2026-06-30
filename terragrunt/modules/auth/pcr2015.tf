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
