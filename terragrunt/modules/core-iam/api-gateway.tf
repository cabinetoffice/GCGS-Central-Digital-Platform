resource "aws_iam_role" "api_gateway_cloudwatch" {
  name               = "${local.name_prefix}-api-gateway-cloudwatch"
  assume_role_policy = data.aws_iam_policy_document.api_gateway_cloudwatch_assume.json

  tags = var.tags
}

resource "aws_iam_policy" "api_gateway_cloudwatch" {
  name        = "${local.name_prefix}-api-gateway-cloudwatch"
  description = "API Gateway access to cloudwatch"
  policy      = data.aws_iam_policy_document.api_gateway_cloudwatch.json
  tags        = var.tags
}

resource "aws_iam_role_policy_attachment" "api_gateway_cloudwatch" {
  policy_arn = aws_iam_policy.api_gateway_cloudwatch.arn
  role       = aws_iam_role.api_gateway_cloudwatch.name
}
