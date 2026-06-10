resource "aws_dynamodb_table" "teams_notifier" {
  name         = local.dynamodb_table_name
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "execution_key"

  attribute {
    name = "execution_key"
    type = "S"
  }

  tags = var.tags
}
