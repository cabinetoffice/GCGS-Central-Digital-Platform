locals {
  codebuild_iam_name      = "${local.name_prefix}-${var.environment}-ci-codebuild"
  name_prefix             = var.product.resource_name
  orchestrator_account_id = var.account_ids["orchestrator"]
  pipeline_iam_name       = "${local.name_prefix}-${var.environment}-ci-pipeline"
  use_codestar_connection = var.environment != "orchestrator"
}
