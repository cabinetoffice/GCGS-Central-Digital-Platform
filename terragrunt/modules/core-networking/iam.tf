resource "aws_cloudwatch_log_resource_policy" "waf_manage_logs" {
  policy_document = data.aws_iam_policy_document.waf_manage_logs.json
  policy_name     = "AWSWAF-LOGS"
}
