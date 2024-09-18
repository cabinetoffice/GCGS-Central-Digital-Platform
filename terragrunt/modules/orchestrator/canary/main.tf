resource "aws_synthetics_canary" "canary" {
  for_each = var.canary_configs

  depends_on = [aws_iam_role_policy_attachment.canary_role_policy_attachment]

  artifact_s3_location = "s3://${module.s3_bucket_canary.bucket}/"
  delete_lambda        = true
  execution_role_arn   = var.role_canary_arn
  handler              = "canary_check_api.handler"
  name                 = "${local.name_prefix}-ver-${substr(each.key, 0 ,3)}"
  runtime_version      = "syn-python-selenium-4.0"
  s3_bucket            = module.s3_bucket_canary.id
  s3_key               = "lambda_package.zip"
  start_canary         = true
  tags                 = var.tags


  run_config {
    environment_variables = {
      API_URL              = each.value.api_url
      AUTH_SECRET_NAME     = "${local.name_prefix}-canary-${each.key}-credentials"
      VERSION_PARAM_NAME   = aws_ssm_parameter.expected_service_versions[each.key].name
      WEB_DRIVER_LOG_LEVEL = "WARNING"
    }
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