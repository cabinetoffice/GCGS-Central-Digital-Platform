locals {
  name_prefix = var.product.resource_name

  effective_mail_from_domains = length(var.mail_from_domains) > 0 ? var.mail_from_domains : [var.product.public_hosted_zone]
}
