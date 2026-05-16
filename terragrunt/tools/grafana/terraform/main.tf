locals {
  dashboard_files = fileset("${path.module}/dashboards", "*.json")
}

resource "grafana_dashboard" "dashboards" {
  for_each = toset(local.dashboard_files)

  config_json = file("${path.module}/dashboards/${each.value}")
}

resource "grafana_contact_point" "teams" {
  count = var.teams_webhook_url != "" ? 1 : 0

  name = var.alert_contact_point_name

  teams {
    url = var.teams_webhook_url
  }
}
