locals {
  codebuild_iam_name      = "${local.name_prefix}-${var.environment}-ci-codebuild"
  name_prefix             = var.product.resource_name
  orchestrator_account_id = var.account_ids["orchestrator"]
  pipeline_iam_name       = "${local.name_prefix}-${var.environment}-ci-pipeline"
  use_codestar_connection = var.environment != "orchestrator"

  pen_testing_user_arns   = contains(["staging", "orchestrator"], var.environment) ? concat(var.pen_testing_user_arns, var.pen_testing_external_user_arns) : var.pen_testing_user_arns
  pen_testing_allowed_ips = contains(["staging", "orchestrator"], var.environment) ? [] : var.pen_testing_allowed_ips
}
