resource "aws_cloudwatch_log_group" "deployments" {
  for_each      = local.deployment_environments

  name              = "/${local.name_prefix}/deploymen/${each.key}"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

resource "aws_cloudwatch_event_rule" "ci_service_version_updated" {

  name        = "${local.name_prefix}-ci-service-version-updated"
  description = "CloudWatch Event rule to detect updating service version"

  event_pattern = jsonencode({
    "source" : ["aws.ssm"],
    "detail-type" : ["AWS API Call via CloudTrail"],
    "detail" : {
      "eventSource" : ["ssm.amazonaws.com"],
      "eventName" : ["PutParameter"],
      "requestParameters" : {
        "name" : ["${local.name_prefix}-service-version"]
      }
    }
  })

  tags = var.tags
}

resource "aws_cloudwatch_event_target" "this" {
  rule     = aws_cloudwatch_event_rule.ci_service_version_updated.name
  arn      = aws_codepipeline.this.arn
  role_arn = var.role_cloudwatch_events_arn
}
