import {
  to = aws_ssm_parameter.service_version
  id = "${local.name_prefix}-service-version"
}

resource "aws_ssm_parameter" "service_version" {
  description    = "The value of this parameter will be set by the GitHub workflow and will act as a trigger to deploy infrastructure and services tagged with the same version"
  insecure_value = ""
  name           = "${local.name_prefix}-service-version"
  tags           = var.tags
  type           = "String"
}
