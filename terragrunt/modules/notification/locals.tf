locals {
  name_prefix = var.product.resource_name

  effective_mail_from_domain = var.mail_from_domain != null ? var.mail_from_domain : var.product.public_hosted_zone
  manage_dns_records         = var.mail_from_domain == null
}
