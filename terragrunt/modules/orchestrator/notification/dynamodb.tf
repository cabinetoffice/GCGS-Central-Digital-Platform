resource "aws_dynamodb_table" "pipeline_execution_timestamps" {
  name           = "${local.name_prefix}-pipeline-execution"
  billing_mode   = "PAY_PER_REQUEST"
  hash_key       = "pipeline_execution_id"

  attribute {
    name = "pipeline_execution_id"
    type = "S"
  }

  tags = var.tags
}
