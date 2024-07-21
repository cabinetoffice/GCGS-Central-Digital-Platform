resource "aws_cloudwatch_log_group" "update_account" {
  name              = "/${local.name_prefix}/build/${local.update_account_cb_name}"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

resource "aws_cloudwatch_log_group" "update_ecs_services" {
  name              = "/${local.name_prefix}/build/${local.update_ecs_service_cb_name}"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

resource "aws_cloudwatch_event_rule" "service_version_ssm_update" {

  name        = "${local.name_prefix}-service-version-updated"
  description = "CloudWatch Event rule to detect updating service version"

  event_pattern = jsonencode({
    "source" : ["aws.ssm"],
    "detail-type" : ["AWS API Call via CloudTrail"],
    "detail" : {
      "eventSource" : ["ssm.amazonaws.com"],
      "eventName" : ["PutParameter"],
      "requestParameters" : {
        "name" : ["cdp-sirsi-service-version"]
      }
    }
  })

  tags = var.tags
}

resource "aws_cloudwatch_event_target" "trigger_service_deployment" {
  rule     = aws_cloudwatch_event_rule.service_version_ssm_update.name
  arn      = "arn:aws:codepipeline:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:cdp-sirsi-trigger-update-ecs-services"
  role_arn = var.role_cloudwatch_events_arn
}
