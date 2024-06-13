resource "aws_cloudwatch_dashboard" "service_deployment" {
  dashboard_name = "${local.name_prefix}-service-deployments"

  dashboard_body = templatefile("${path.module}/templates/dashboard-service-deployment.json.tftpl",
    {
      account_id = data.aws_caller_identity.current.account_id

    }
  )
}

resource "aws_cloudwatch_dashboard" "logs_warn_err" {
  dashboard_name = "${local.name_prefix}-warn-err-logs"

  dashboard_body = templatefile("${path.module}/templates/dashboard-logs-warn-err.json.tftpl",
    {
      flat_service_widgets = local.flat_service_widgets
    }
  )
}
