locals {
  dashboard_files = fileset("${path.module}/dashboards", "**/*.json")
  dashboard_folder_map = {
    application   = grafana_folder.application.uid
    infrastructure = grafana_folder.infrastructure.uid
    overview      = grafana_folder.overview.uid
    traffic       = grafana_folder.traffic.uid
  }
  dashboard_content = {
    for file_path in local.dashboard_files : file_path => replace(
      replace(
        file("${path.module}/dashboards/${file_path}"),
        "GRAFANA_CLOUDWATCH_ACCOUNT_ID",
        var.cloudwatch_account_id
      ),
      "CDP_SIRSI_ENVIRONMENT",
      var.environment
    )
  }
}

resource "grafana_dashboard" "dashboards" {
  for_each = toset(local.dashboard_files)

  folder_uid  = local.dashboard_folder_map[split("/", each.value)[0]]
  config_json = local.dashboard_content[each.value]
}
