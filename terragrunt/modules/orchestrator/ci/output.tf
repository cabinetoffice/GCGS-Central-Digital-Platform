output "event_rule_ci_service_version_updated_name" {
  value = aws_cloudwatch_event_rule.ci_service_version_updated.name
}

output "ssm_service_version_arn" {
  value = aws_ssm_parameter.service_version.arn
}

output "ssm_service_version_name" {
  value = aws_ssm_parameter.service_version.name
}

output "deployment_pipeline_name" {
  value = aws_codepipeline.this.name
}
