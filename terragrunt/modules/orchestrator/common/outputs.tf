output "ssm_envs_service_version_arn" {
  value = aws_ssm_parameter.service_versions.arn
}

output "ssm_envs_service_version_name" {
  value = aws_ssm_parameter.service_versions.name
}
