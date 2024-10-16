locals {

  name_prefix = var.product.resource_name

  services = {
    for name, config in var.service_configs :
    name => config if !contains([
      "entity-verification-migrations",
      "organisation-information-migrations",
    "organisation-app"], config.name)
  }

  endpoints = [
    for service in local.services :
    {
      name       = service.name,
      url        = "https://${aws_api_gateway_domain_name.ecs_api.domain_name}/${service.name}/swagger/index.html"
      direct_url = "https://${service.name}.${var.public_hosted_zone_fqdn}/swagger/index.html"
    }
  ]

  orchestrator_account_id = var.account_ids["orchestrator"]

  now            = timestamp()
  year           = substr(local.now, 0, 4)
  month          = substr(local.now, 5, 2)
  day            = substr(local.now, 8, 2)
  hour           = substr(local.now, 11, 2)
  minute         = substr(local.now, 14, 2)
  second         = substr(local.now, 17, 2)
  uk_hour        = tonumber(local.hour) + 1
  formatted_hour = format("%02d", local.uk_hour % 24)
  formatted_date = "${local.day}-${local.month}-${local.year} ${local.formatted_hour}:${local.minute}:${local.second}"

}
