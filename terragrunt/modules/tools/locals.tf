locals {
  name_prefix = var.product.resource_name

  orchestrator_account_id = var.account_ids["orchestrator"]

  auto_redeploy_tools_service_configs = {
    for name, config in var.tools_configs :
    config.name => config if !contains(["clamav-rest", "grafana", "healthcheck", "k6"], config.name)
  }

  auto_redeploy_tools_tasks = [
    for name, config in local.auto_redeploy_tools_service_configs :
    config.name
  ]

  executable_tasks_by_step_functions = concat(local.auto_redeploy_tools_tasks, ["k6"])

  cloud_beaver_container_path = "/opt/cloudbeaver/workspace"
  cloud_beaver_volume_name    = "workspace"

  filestash_reports_bucket_name = coalesce(
    var.filestash_reports_bucket_name,
    "${local.name_prefix}-${var.environment}-e2e-nightly-dev-reports-${data.aws_caller_identity.current.account_id}"
  )
  filestash_reports_bucket_arn = coalesce(
    var.filestash_reports_bucket_arn,
    "arn:aws:s3:::${local.filestash_reports_bucket_name}"
  )
  filestash_reports_bucket_kms_key_arn = var.filestash_reports_bucket_kms_key_arn

  tools_alb_ports = distinct([
    for name, config in var.tools_configs :
    config.port if config.name != "k6"
  ])

}
