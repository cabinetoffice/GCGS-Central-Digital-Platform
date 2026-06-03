data "aws_secretsmanager_secret" "grafana_alerting" {
  name = "cdp-sirsi-grafana-alerting-webhook"
}

data "aws_secretsmanager_secret_version" "grafana_alerting" {
  secret_id = data.aws_secretsmanager_secret.grafana_alerting.id
}

locals {
  teams_webhook_url = try(
    jsondecode(data.aws_secretsmanager_secret_version.grafana_alerting.secret_string).TEAMS_WEBHOOK_URL,
    ""
  )
}
