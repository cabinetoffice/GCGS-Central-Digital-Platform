locals {

  name_prefix = var.product.resource_name
  domain_name = local.name_prefix

  domain_arn = "arn:aws:es:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:domain/${local.domain_name}"

  opensearch_access_principals = [
      "arn:aws:iam::${data.aws_caller_identity.current.account_id}:role/bootstrap",
      var.role_opensearch_admin_arn
    ]

  private_subnet_ids = var.is_production ? var.private_subnet_ids : slice(var.private_subnet_ids, 0, 2)
}
