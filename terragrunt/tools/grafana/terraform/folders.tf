resource "grafana_folder" "application" {
  uid   = "application"
  title = "Application"
}

resource "grafana_folder" "infrastructure" {
  uid   = "infrastructure"
  title = "Infrastructure"
}

resource "grafana_folder" "overview" {
  uid   = "overview"
  title = "Overview"
}

resource "grafana_folder" "traffic" {
  uid   = "traffic"
  title = "Traffic"
}

resource "grafana_folder" "alerts" {
  uid   = "alerts"
  title = "Alerts"

  lifecycle {
    precondition {
      condition     = var.environment != "production" || local.teams_webhook_url != ""
      error_message = "Grafana production alerting requires secret `cdp-sirsi-grafana-alerting-webhook` with JSON key `TEAMS_WEBHOOK_URL`."
    }
  }
}
