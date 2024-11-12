data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_secretsmanager_secret" "pen_testing_configuration" {
  name = "${local.name_prefix}-pen-testing-configuration"
}

data "aws_secretsmanager_secret_version" "pen_testing_configuration" {
  secret_id = data.aws_secretsmanager_secret.pen_testing_configuration.id
}

data "aws_secretsmanager_secret" "terraform_operators" {
  name = "${local.name_prefix}-terraform-operators"
}

data "aws_secretsmanager_secret_version" "terraform_operators" {
  secret_id = data.aws_secretsmanager_secret.terraform_operators.id
}
