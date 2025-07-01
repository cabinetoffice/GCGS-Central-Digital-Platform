resource "aws_cloudwatch_log_group" "grafana" {
  # kms_key_id      = var.ecs_cloudwatch_kms_key_id
  name              = "/ecs/grafana"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

resource "aws_cloudwatch_dashboard" "service_deployment" {
  dashboard_name = "${local.name_prefix}-service-deployments"

  dashboard_body = templatefile("${path.module}/templates/cloudwatch-dashboards/service-deployment.json.tftpl",
    {
      account_id = data.aws_caller_identity.current.account_id

    }
  )
}

resource "aws_cloudwatch_dashboard" "logs_warn_err" {
  dashboard_name = "${local.name_prefix}-warn-err-logs"

  dashboard_body = templatefile("${path.module}/templates/cloudwatch-dashboards/logs-warn-err.json.tftpl",
    {
      flat_service_widgets = local.flat_service_widgets
    }
  )
}
