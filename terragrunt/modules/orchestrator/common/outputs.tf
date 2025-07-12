output "ssm_envs_combined_service_version_arn" {
  value = aws_ssm_parameter.service_versions.arn
}

output "ssm_envs_combined_service_version_name" {
  value = aws_ssm_parameter.service_versions.name
}

output "ssm_envs_cfs_service_version_arn" {
  value = aws_ssm_parameter.service_versions_cfs.arn
}

output "ssm_envs_cfs_service_version_name" {
  value = aws_ssm_parameter.service_versions_cfs.name
}

output "ssm_envs_fts_service_version_arn" {
  value = aws_ssm_parameter.service_versions_fts.arn
}

output "ssm_envs_fts_service_version_name" {
  value = aws_ssm_parameter.service_versions_fts.name
}

output "ssm_envs_sirsi_service_version_arn" {
  value = aws_ssm_parameter.service_versions_sirsi.arn
}

output "ssm_envs_sirsi_service_version_name" {
  value = aws_ssm_parameter.service_versions_sirsi.name
}
