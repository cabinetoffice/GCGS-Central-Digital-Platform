locals {

  base_host_headers     = var.is_frontend_app ? local.tg_host_header_with_alias : local.tg_host_header
  internal_host_headers = var.internal_domain == null ? [] : ["${var.name}.${var.internal_domain}"]

  host_headers = compact(
    concat(
      local.base_host_headers,
      var.extra_host_headers
    )
  )

  internal_tg_name_prefix        = "${substr(local.tg_name_prefix, 0, 5)}i"
  listener_name                  = "cdp-${coalesce(var.listener_name, var.name)}"
  service_listener_rule_priority = var.listener_priority
  tg_host_header                 = var.public_domain == null ? [] : ["${var.name}.${var.public_domain}"]
  tg_host_header_with_alias      = var.public_domain == null ? [] : ["${var.name}.${var.public_domain}", var.public_domain]
  tg_name_prefix                 = substr(replace(local.listener_name, "cdp-", ""), 0, 6)
}
