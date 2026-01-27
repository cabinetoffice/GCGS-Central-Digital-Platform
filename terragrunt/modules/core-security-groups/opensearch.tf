resource "aws_security_group" "opensearch" {
  description = "Security group to be attached to the OpenSearch"
  name        = "${local.name_prefix}-opensearch"
  vpc_id      = var.vpc_id

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-opensearch"
    }
  )
}
