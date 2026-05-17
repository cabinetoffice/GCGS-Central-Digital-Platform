locals {
  dashboard_files = fileset("${path.module}/dashboards", "**/*.json")
  dashboard_folder_map = {
    application   = grafana_folder.application.uid
    infrastructure = grafana_folder.infrastructure.uid
    overview      = grafana_folder.overview.uid
    traffic       = grafana_folder.traffic.uid
  }
  dashboard_folder_short_map = {
    application   = "app"
    infrastructure = "infra"
    overview      = "overview"
    traffic       = "traffic"
  }
  dashboard_base_name = {
    for file_path in local.dashboard_files : file_path => trimsuffix(basename(file_path), ".json")
  }
  dashboard_uid_raw = {
    for file_path in local.dashboard_files :
    file_path => "${local.dashboard_folder_short_map[split("/", file_path)[0]]}-${local.dashboard_base_name[file_path]}"
  }
  dashboard_uid = {
    for file_path in local.dashboard_files :
    file_path => length(local.dashboard_uid_raw[file_path]) > 40
      ? "${local.dashboard_folder_short_map[split("/", file_path)[0]]}-${substr(local.dashboard_base_name[file_path], 0, 40 - length(local.dashboard_folder_short_map[split("/", file_path)[0]]) - 1)}"
      : local.dashboard_uid_raw[file_path]
  }
  dashboard_content_raw = {
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
  dashboard_content = {
    for file_path, content in local.dashboard_content_raw : file_path => merge(
      jsondecode(content),
      {
        uid  = local.dashboard_uid[file_path]
        tags = distinct(concat(try(jsondecode(content).tags, []), ["terraform-managed"]))
      }
    )
  }
}

resource "grafana_dashboard" "dashboards" {
  for_each = toset(local.dashboard_files)

  folder      = local.dashboard_folder_map[split("/", each.value)[0]]
  config_json = jsonencode(local.dashboard_content[each.value])

  depends_on = [
    grafana_folder.application,
    grafana_folder.infrastructure,
    grafana_folder.overview,
    grafana_folder.traffic,
  ]
}
