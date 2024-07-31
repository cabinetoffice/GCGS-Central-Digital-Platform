resource "aws_cloudwatch_event_rule" "rds_credentials_rotation" {
  name        = "${var.db_name}-rds-creds-rotation"
  description = "CloudWatch Event rule to detect rotation of the RDS credentials for ${var.db_name}"

  event_pattern = jsonencode(
    {
      "source" : ["aws.secretsmanager"],
      "detail-type" : ["AWS API Call via CloudTrail"],
      "detail" : {
        "eventSource" : ["secretsmanager.amazonaws.com"],
        "eventName" : ["UpdateSecretVersionStage"],
        "requestParameters" : {
          "secretId" : [data.aws_secretsmanager_secret.postgres.arn]
        }
      }
    }
  )

  tags = var.tags
}

resource "aws_cloudwatch_event_target" "update_connection_string" {
  rule     = aws_cloudwatch_event_rule.rds_credentials_rotation.name
  arn      = aws_sfn_state_machine.update_connection_string.arn
  role_arn = var.role_cloudwatch_events_arn
}
