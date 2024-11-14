locals {
  tg_host_header                 = ["${var.name}.${var.public_domain}"]
  tg_host_header_with_alias      = ["${var.name}.${var.public_domain}", var.public_domain]
  service_listener_rule_priority = var.family == "app" ? var.host_port - 8000 : var.host_port
}
