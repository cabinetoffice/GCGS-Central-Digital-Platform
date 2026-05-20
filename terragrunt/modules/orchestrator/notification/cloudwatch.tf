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

resource "aws_cloudwatch_event_connection" "teams_webhook" {
  name               = "${local.name_prefix}-teams-webhook"
  description        = "Allow connection to Microsoft Teams webhook"
  authorization_type = "BASIC"

  auth_parameters {
    basic {
      username = "teams"
      password = "not-used"
    }
  }
}

resource "aws_cloudwatch_event_target" "deployment_codebuild_teams_notification" {
  arn      = aws_sfn_state_machine.teams_notification.arn
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


resource "aws_cloudwatch_event_target" "deployment_ecs_teams_notification" {
  arn      = aws_sfn_state_machine.teams_notification.arn
  role_arn = var.role_cloudwatch_events_arn
  rule     = aws_cloudwatch_event_rule.deployment_ecs.name
}

resource "aws_cloudwatch_event_rule" "deployment_pipeline" {

  name        = "${local.name_prefix}-ci-deployment-pipeline-unified"
  description = "CloudWatch Event rule to detect deployment pipeline events"

  event_pattern = jsonencode({
    "source" : ["aws.codepipeline"],
    "detail-type" : ["CodePipeline Action Execution State Change"],
  })

  tags = var.tags
}

resource "aws_cloudwatch_event_target" "deployment_pipeline_teams_notification" {
  arn      = aws_sfn_state_machine.teams_notification_middleman.arn
  role_arn = var.role_cloudwatch_events_arn
  rule     = aws_cloudwatch_event_rule.deployment_pipeline.name
}
