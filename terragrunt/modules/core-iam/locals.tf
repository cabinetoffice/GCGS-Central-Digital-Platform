# Note!
# Resources in this file are shared with orchestrator/iam module

locals {
  name_prefix             = var.product.resource_name
  orchestrator_account_id = var.account_ids["orchestrator"]
}
