locals {

  base_host_headers = var.is_frontend_app ? local.tg_host_header_with_alias : local.tg_host_header

  host_headers = compact(
    concat(
      local.base_host_headers,
      var.extra_host_headers
    )
  )

  listener_name                  = "cdp-${coalesce(var.listener_name, var.name)}"
  tg_name_prefix                 = substr("cdp-${coalesce(var.listener_name, var.name)}", 0, 6)
  tg_host_header                 = ["${var.name}.${var.public_domain}"]
  tg_host_header_with_alias      = ["${var.name}.${var.public_domain}", var.public_domain]
  service_listener_rule_priority = var.family == "app" ? var.host_port - 8000 : var.host_port
}
