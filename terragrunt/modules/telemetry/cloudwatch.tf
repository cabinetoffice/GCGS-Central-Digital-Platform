resource "aws_cloudwatch_dashboard" "service_deployment" {
  dashboard_name = "${local.name_prefix}-service-deployments"

  dashboard_body = templatefile("${path.module}/templates/dashboard-service-deployment.json.tftpl",
    {
      account_id = data.aws_caller_identity.current.account_id

    }
  )
}
