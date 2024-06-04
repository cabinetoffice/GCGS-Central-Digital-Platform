locals {
  tg_host_header            = ["${var.name}.${var.product.public_hosted_zone}"]
  tg_host_header_with_alias = ["${var.name}.${var.product.public_hosted_zone}", var.product.public_hosted_zone]
}
