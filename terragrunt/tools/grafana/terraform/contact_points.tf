resource "grafana_contact_point" "teams" {
  count = var.teams_webhook_url != "" ? 1 : 0

  name = var.alert_contact_point_name

  teams {
    url = var.teams_webhook_url
  }
}
