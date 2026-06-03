data "aws_secretsmanager_secret_version" "grafana_alerting" {
  secret_id = "cdp-sirsi-grafana-alerting-webhook"
}

locals {
  teams_webhook_url = try(
    jsondecode(data.aws_secretsmanager_secret_version.grafana_alerting.secret_string).TEAMS_WEBHOOK_URL,
    ""
  )
}
