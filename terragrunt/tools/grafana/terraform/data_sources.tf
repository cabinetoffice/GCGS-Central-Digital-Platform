resource "grafana_data_source" "cloudwatch" {
  name = "Cloudwatch"
  type = "cloudwatch"
  uid  = "CLOUDWATCH"

  is_default = false

  json_data_encoded = jsonencode({
    defaultRegion = "eu-west-2"
    authType      = "default"
    assumeRoleArn = var.cloudwatch_assume_role_arn
  })
}
