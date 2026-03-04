provider "aws" {
  alias  = "virginia"
  region = "us-east-1"
}

resource "aws_acm_certificate" "docs" {
  count             = var.docs_domain_name == null ? 0 : 1
  domain_name       = var.docs_domain_name
  provider          = aws.virginia
  validation_method = "DNS"

  tags = merge(var.tags, { Name = var.docs_domain_name })

  lifecycle {
    create_before_destroy = true
  }
}
