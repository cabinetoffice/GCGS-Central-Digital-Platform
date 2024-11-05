resource "aws_cloudwatch_event_connection" "slack" {
  name               = "${local.name_prefix}-goaco-slack"
  description        = "Allow connection to GOACO's slack private channel"
  authorization_type = "BASIC"


  auth_parameters {
    basic {
      username = "authentication"
      password = "is not needed"
    }
  }
}

resource "aws_cloudwatch_event_connection" "slack_unified_notification" {
  name               = "${local.name_prefix}-goaco-slack-unified-notification"
  description        = "Allow connection to GOACO's slack API"
  authorization_type = "API_KEY"


  auth_parameters {
    api_key {
      key   = "Authorization"
      value = local.slack_api_auth
    }
  }
}

resource "aws_cloudwatch_event_target" "service_version_slack_notification" {
  arn      = aws_sfn_state_machine.slack_notification.arn
  role_arn = var.role_cloudwatch_events_arn
  rule     = var.event_rule_ci_service_version_updated_name
}

resource "aws_cloudwatch_event_rule" "deployment_pipeline" {

  name        = "${local.name_prefix}-ci-deployment-pipeline"
  description = "CloudWatch Event rule to detect deployment Pipeline events"

  event_pattern = jsonencode({
    "source" : ["aws.codepipeline"],
    "detail-type" : [
      "CodePipeline Stage Execution State Change",
    ],
  })

  tags = var.tags
}

resource "aws_cloudwatch_event_target" "deployment_pipeline_slack_notification" {
  arn      = aws_sfn_state_machine.slack_notification.arn
  role_arn = var.role_cloudwatch_events_arn
  rule     = aws_cloudwatch_event_rule.deployment_pipeline.name
}

resource "aws_cloudwatch_event_rule" "deployment_codebuild" {

  name        = "${local.name_prefix}-ci-deployment-codebuild"
  description = "CloudWatch Event rule to detect deployment Codebuild events"

  event_pattern = jsonencode({
    "source" : ["aws.codebuild"],
    "detail-type" : ["CodeBuild Build State Change"],
    "detail" : {
      "build-status" : ["IN_PROGRESS", "SUCCEEDED", "STOPPED", "FAILED"],
    }
  })

  tags = var.tags
}

resource "aws_cloudwatch_event_target" "deployment_codebuild_slack_notification" {
  arn      = aws_sfn_state_machine.slack_notification.arn
  role_arn = var.role_cloudwatch_events_arn
  rule     = aws_cloudwatch_event_rule.deployment_codebuild.name
}

resource "aws_cloudwatch_event_rule" "deployment_ecs" {

  name        = "${local.name_prefix}-ci-deployment-ecs"
  description = "CloudWatch Event rule to detect deployment ECS events"

  event_pattern = jsonencode({
    "source" : ["aws.ecs"],
    "detail-type" : ["ECS Task State Change", "ECS Container Instance State Change", "ECS Deployment State Change"],
  })

  tags = var.tags
}


resource "aws_cloudwatch_event_target" "deployment_ecs_slack_notification" {
  arn      = aws_sfn_state_machine.slack_notification.arn
  role_arn = var.role_cloudwatch_events_arn
  rule     = aws_cloudwatch_event_rule.deployment_ecs.name
}

resource "aws_cloudwatch_event_rule" "deployment_pipeline_unified_slack_notification" {

  name        = "${local.name_prefix}-ci-deployment-pipeline-unified"
  description = "CloudWatch Event rule to detect deployment pipeline events"

  event_pattern = jsonencode({
    "source" : ["aws.codepipeline"],
    "detail-type" : ["CodePipeline Action Execution State Change"],
  })

  tags = var.tags
}

resource "aws_cloudwatch_event_target" "deployment_pipeline_unified_slack_notification" {
  arn      = aws_sfn_state_machine.slack_unified_notification.arn
  role_arn = var.role_cloudwatch_events_arn
  rule     = aws_cloudwatch_event_rule.deployment_pipeline_unified_slack_notification.name
}
