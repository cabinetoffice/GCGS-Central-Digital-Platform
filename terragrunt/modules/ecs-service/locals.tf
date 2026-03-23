locals {

  base_host_headers     = var.is_frontend_app ? local.tg_host_header_with_alias : local.tg_host_header
  internal_host_headers = var.internal_domain == null ? [] : ["${var.name}.${var.internal_domain}"]

  host_headers = compact(
    concat(
      local.base_host_headers,
      var.extra_host_headers
    )
  )

  listener_name                  = "cdp-${coalesce(var.listener_name, var.name)}"
  service_listener_rule_priority = var.listener_priority
  external_listener_actions = var.user_pool_arn != null && var.user_pool_client_id != null && var.user_pool_domain != null ? [
    {
      order = 1
      type  = "authenticate-cognito"
    },
    {
      order = 2
      type  = "forward"
    }
    ] : [
    {
      order = 1
      type  = "forward"
    }
  ]
  tg_base_name_raw    = replace(local.listener_name, "cdp-", "")
  tg_base_name_abbrev = replace(
    replace(
      replace(
        replace(local.tg_base_name_raw, "commercial-tools-", "cml-tls-"),
        "user-management-",
        "usr-mgt-"
      ),
      "outbox-processor-",
      "obx-prc-"
    ),
    "organisation-",
    "org-"
  )
  tg_base_name           = substr(local.tg_base_name_abbrev, 0, 21)
  tg_base_name_sanitized = trim(local.tg_base_name, "-")
  tg_suffix                      = coalesce(var.tg_suffix, substr(md5(join(":", [
    local.listener_name,
    tostring(coalesce(var.service_port, 0)),
    var.internal_alb_enabled ? "int" : "ext"
  ])), 0, 5))
  tg_name                        = "${local.tg_base_name_sanitized}-${local.tg_suffix}"
  internal_tg_name               = "${substr(local.tg_base_name_sanitized, 0, 16)}-i-${local.tg_suffix}"
  tg_host_header                 = var.public_domain == null ? [] : ["${var.name}.${var.public_domain}"]
  tg_host_header_with_alias      = var.public_domain == null ? [] : ["${var.name}.${var.public_domain}", var.public_domain]
}
