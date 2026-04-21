output "event_rule_ci_sirsi_service_version_updated_name" {
  value = aws_cloudwatch_event_rule.ci_sirsi_service_version_updated.name
}

output "event_rule_ci_cfs_service_version_updated_name" {
  value = aws_cloudwatch_event_rule.ci_cfs_service_version_updated.name
}

output "ssm_service_version_cfs_arn" {
  value = aws_ssm_parameter.service_version_cfs.arn
}

output "ssm_service_version_cfs_name" {
  value = aws_ssm_parameter.service_version_cfs.name
}

output "event_rule_ci_fts_service_version_updated_name" {
  value = aws_cloudwatch_event_rule.ci_fts_service_version_updated.name
}

output "ssm_service_version_fts_arn" {
  value = aws_ssm_parameter.service_version_fts.arn
}

output "ssm_service_version_fts_name" {
  value = aws_ssm_parameter.service_version_fts.name
}

output "ssm_service_version_sirsi_arn" {
  value = aws_ssm_parameter.service_version_sirsi.arn
}

output "ssm_service_version_sirsi_name" {
  value = aws_ssm_parameter.service_version_sirsi.name
}

output "deployment_pipeline_name" {
  value = aws_codepipeline.this.name
}

output "fts_test_db_backup_bucket_name" {
  value = module.fts_db_backup_bucket.bucket
}

output "fts_test_db_backup_bucket_arn" {
  value = module.fts_db_backup_bucket.arn
}

output "fts_github_oidc_role_arn" {
  value = aws_iam_role.fts_github_oidc_read_db_backup.arn
}

output "fts_github_oidc_role_name" {
  value = aws_iam_role.fts_github_oidc_read_db_backup.name
}
