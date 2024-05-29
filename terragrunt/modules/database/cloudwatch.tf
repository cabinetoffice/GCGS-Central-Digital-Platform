resource "aws_cloudwatch_event_rule" "rds_credentials_rotation" {
  name        = "${local.name_prefix}-rds-credentials-rotation"
  description = "CloudWatch Event rule to detect rotation of the RDS credentials"

  event_pattern = jsonencode(
    {
      "source": ["aws.secretsmanager"],
      "detail-type": ["AWS API Call via CloudTrail"],
      "detail": {
        "eventSource": ["secretsmanager.amazonaws.com"],
        "eventName": ["RotateSecret"],
        "requestParameters": {
          "secretId": [data.aws_secretsmanager_secret.postgres.arn]
        }
      }
    }
  )

  tags = var.tags
}
