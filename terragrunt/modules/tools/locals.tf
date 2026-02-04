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


  rds_creds_sirsi = jsondecode(data.aws_secretsmanager_secret_version.rds_creds_sirsi.secret_string)
  rds_creds_ev    = jsondecode(data.aws_secretsmanager_secret_version.rds_creds_ev.secret_string)
  rds_creds_cfs   = jsondecode(data.aws_secretsmanager_secret_version.rds_creds_cfs.secret_string)
  rds_creds_fts   = jsondecode(data.aws_secretsmanager_secret_version.rds_creds_fts.secret_string)
}
