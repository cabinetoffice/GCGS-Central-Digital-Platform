locals {
  codebuild_iam_name      = "${local.name_prefix}-${var.environment}-ci-codebuild"
  name_prefix             = var.product.resource_name
  orchestrator_account_id = var.account_ids["orchestrator"]
  pipeline_iam_name       = "${local.name_prefix}-${var.environment}-ci-pipeline"

  pen_testing_config  = jsondecode(data.aws_secretsmanager_secret_version.pen_testing_configuration.secret_string)
  terraform_operators = concat(
    jsondecode(data.aws_secretsmanager_secret_version.terraform_operators.secret_string)["operators"],
    var.environment == "development" ? ["arn:aws:iam::043309357622:user/eran-inventur"] : []
  )

  pen_testing_config_allowed_ips        = local.pen_testing_config["allowed_ips"]
  pen_testing_config_external_user_arns = local.pen_testing_config["external_user_arns"]
  pen_testing_config_user_arns          = local.pen_testing_config["user_arns"]

  pen_testing_user_arns   = contains(["no-account-allowed list them here if needed"], var.environment) ? concat(local.pen_testing_config_user_arns, local.pen_testing_config_external_user_arns) : local.pen_testing_config_user_arns
  pen_testing_allowed_ips = contains(["no-account-allowed list them here if needed"], var.environment) ? [] : local.pen_testing_config_allowed_ips
}
