locals {
  dashboard_files_all = fileset("${path.module}/dashboards", "**/*.json")
  dashboard_files = [
    for file_path in local.dashboard_files_all : file_path
    if !(startswith(file_path, "application/logs-") && file_path != "application/logs-critical.json")
  ]
  dashboard_folder_map = {
    application    = grafana_folder.application.uid
    infrastructure = grafana_folder.infrastructure.uid
    overview       = grafana_folder.overview.uid
    traffic        = grafana_folder.traffic.uid
    alerts         = grafana_folder.alerts.uid
  }
  dashboard_folder_short_map = {
    application    = "app"
    infrastructure = "infra"
    overview       = "overview"
    traffic        = "traffic"
    alerts         = "alerts"
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

  log_dashboard_defs = jsondecode(file("${path.module}/data/log_dashboards.json"))
  log_dashboard_templates = {
    single = "${path.module}/dashboards/templates/logs-single.json.tftpl"
  }
  log_health_filter = "\n| filter RequestPath != \"/health\""
  log_expression_filtered = {
    for service, cfg in local.log_dashboard_defs : service => (
      length(regexall("RequestPath", cfg.type == "multi" ? cfg.all_expression : cfg.expression)) > 0
        ? (
            length(regexall("\n\\| sort", cfg.type == "multi" ? cfg.all_expression : cfg.expression)) > 0
              ? replace(cfg.type == "multi" ? cfg.all_expression : cfg.expression, "\n| sort", "${local.log_health_filter}\n| sort")
              : "${cfg.type == "multi" ? cfg.all_expression : cfg.expression}${local.log_health_filter}"
          )
        : (cfg.type == "multi" ? cfg.all_expression : cfg.expression)
    )
  }
  log_dashboard_uid_raw = {
    for service, cfg in local.log_dashboard_defs : service => "app-logs-${service}"
  }
  log_dashboard_uid = {
    for service, raw_uid in local.log_dashboard_uid_raw :
    service => length(raw_uid) > 40 ? substr(raw_uid, 0, 40) : raw_uid
  }
  log_dashboard_rendered = {
    for service, cfg in local.log_dashboard_defs : service => templatefile(
      local.log_dashboard_templates["single"],
      merge(
        {
          title                   = jsonencode(cfg.title)
          log_group_name          = cfg.log_group_name
          cloudwatch_account_id   = var.cloudwatch_account_id
          expression              = jsonencode(local.log_expression_filtered[service])
        }
      )
    )
  }
  log_dashboard_content = {
    for service, rendered in local.log_dashboard_rendered : service => merge(
      jsondecode(rendered),
      {
        uid  = local.log_dashboard_uid[service]
        tags = distinct(concat(try(jsondecode(rendered).tags, []), ["terraform-managed"]))
      }
    )
  }

  logs_investigation_log_groups = [
    for cfg in local.log_dashboard_defs : {
      accountId = var.cloudwatch_account_id
      arn       = "arn:aws:logs:eu-west-2:${var.cloudwatch_account_id}:log-group:${cfg.log_group_name}:*"
      name      = cfg.log_group_name
    }
  ]
  logs_investigation_group_values = join(
    ",",
    sort(distinct([for cfg in local.log_dashboard_defs : cfg.log_group_name]))
  )
  logs_investigation_rendered = templatefile(
    "${path.module}/dashboards/templates/logs-investigation.json.tftpl",
    {
      title                 = jsonencode("_Logs Investigation")
      cloudwatch_account_id = var.cloudwatch_account_id
      log_groups            = jsonencode(local.logs_investigation_log_groups)
      log_group_values      = jsonencode(local.logs_investigation_group_values)
    }
  )
  logs_investigation_content = merge(
    jsondecode(local.logs_investigation_rendered),
    {
      uid  = "app-logs-investigation"
      tags = distinct(concat(try(jsondecode(local.logs_investigation_rendered).tags, []), ["terraform-managed"]))
    }
  )
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
    grafana_folder.alerts,
  ]
}

resource "grafana_dashboard" "log_dashboards" {
  for_each = local.log_dashboard_content

  folder      = grafana_folder.application.uid
  config_json = jsonencode(each.value)

  depends_on = [
    grafana_folder.application,
  ]
}

resource "grafana_dashboard" "logs_investigation" {
  folder      = grafana_folder.application.uid
  config_json = jsonencode(local.logs_investigation_content)

  depends_on = [
    grafana_folder.application,
  ]
}
