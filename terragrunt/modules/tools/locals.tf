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

  cloud_beaver_data_sources_json = templatefile("${path.module}/templates/service-configs/cloud-beaver-data-sources.json.tftpl", {
    db_sirsi_cluster_address  = var.db_sirsi_cluster_address
    db_sirsi_cluster_port     = var.db_sirsi_cluster_port
    db_sirsi_cluster_name     = var.db_sirsi_cluster_name
    db_sirsi_cluster_username = local.rds_creds_sirsi["username"]
    db_sirsi_cluster_password = local.rds_creds_sirsi["password"]

    db_entity_verification_cluster_address  = var.db_ev_cluster_address
    db_entity_verification_cluster_port     = var.db_sirsi_cluster_port
    db_entity_verification_cluster_name     = var.db_ev_cluster_name
    db_entity_verification_cluster_username = local.rds_creds_ev["username"]
    db_entity_verification_cluster_password = local.rds_creds_ev["password"]

    db_cfs_cluster_address  = var.db_cfs_cluster_address
    db_cfs_cluster_port     = var.db_cfs_cluster_port
    db_cfs_cluster_name     = var.db_cfs_cluster_name
    db_cfs_cluster_username = local.rds_creds_cfs["username"]
    db_cfs_cluster_password = local.rds_creds_cfs["password"]

    db_fts_cluster_address  = var.db_fts_cluster_address
    db_fts_cluster_port     = var.db_fts_cluster_port
    db_fts_cluster_name     = var.db_fts_cluster_name
    db_fts_cluster_username = local.rds_creds_fts["username"]
    db_fts_cluster_password = local.rds_creds_fts["password"]
  })
}
