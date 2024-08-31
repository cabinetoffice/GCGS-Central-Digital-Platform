resource "aws_iam_role" "rds_cloudwatch_role" {
  name = "${local.name_prefix}-rds-cloudwatch"

  assume_role_policy = data.aws_iam_policy_document.rds_cloudwatch_assume.json
}

resource "aws_iam_role_policy_attachment" "rds_monitoring_role_attachment" {
  role       = aws_iam_role.rds_cloudwatch_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonRDSEnhancedMonitoringRole"
}
