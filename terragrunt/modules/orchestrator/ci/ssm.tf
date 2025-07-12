import {
  to = aws_ssm_parameter.service_version_sirsi
  id = "${local.name_prefix}-service-version"
}

resource "aws_ssm_parameter" "service_version_sirsi" {
  description    = "The value of this parameter will be set by the GitHub workflow and will act as a trigger to deploy infrastructure and services tagged with the same version"
  insecure_value = ""
  name           = "${local.name_prefix}-service-version"
  tags           = var.tags
  type           = "String"
}

import {
  to = aws_ssm_parameter.service_version_cfs
  id = "${local.name_prefix}-cfs-service-version"
}

resource "aws_ssm_parameter" "service_version_cfs" {
  description    = "The value of this parameter will be set by the GitHub workflow and will act as a trigger to deploy infrastructure and services tagged with the same version"
  insecure_value = ""
  name           = "${local.name_prefix}-cfs-service-version"
  tags           = var.tags
  type           = "String"
}

import {
  to = aws_ssm_parameter.service_version_fts
  id = "${local.name_prefix}-fts-service-version"
}

resource "aws_ssm_parameter" "service_version_fts" {
  description    = "The value of this parameter will be set by the GitHub workflow and will act as a trigger to deploy infrastructure and services tagged with the same version"
  insecure_value = ""
  name           = "${local.name_prefix}-fts-service-version"
  tags           = var.tags
  type           = "String"
}
