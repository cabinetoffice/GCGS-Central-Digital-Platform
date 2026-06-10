locals {
  name_prefix         = var.product.resource_name
  dynamodb_table_name = var.dynamodb_table_name != "" ? var.dynamodb_table_name : "${local.name_prefix}-teams-notifier-messages"
}
