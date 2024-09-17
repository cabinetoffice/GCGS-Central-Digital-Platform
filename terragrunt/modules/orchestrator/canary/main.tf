resource "aws_synthetics_canary" "canary" {
  depends_on = [
    aws_iam_role_policy_attachment.canary_role_policy_attachment,
#     data.archive_file.canary_script
  ]

  artifact_s3_location = "s3://${module.s3_bucket_canary.bucket}/"
  delete_lambda        = true
  execution_role_arn   = var.role_canary_arn
  handler              = "canary_check_api.handler"
  name                 = local.canary_name
  runtime_version      = "syn-python-selenium-4.0"
#   zip_file             = data.archive_file.canary_script.output_path
#   zip_file             = "test-fixtures/canary_check_api.zip"
  s3_bucket            = module.s3_bucket_canary.id
  s3_key               = "lambda_package.zip"
  start_canary         = true
  tags                 = var.tags


  run_config {
    environment_variables = var.environment_variables
    timeout_in_seconds    = var.canary_timeout_seconds
    memory_in_mb          = var.memory_in_mb
  }
  schedule {
    expression = "rate(1 minute)"
  }

  vpc_config {
    security_group_ids = [var.canary_sg_id]
    subnet_ids         = var.private_subnet_ids
  }
}

output "test" {
  value = module.s3_bucket_canary.bucket
}