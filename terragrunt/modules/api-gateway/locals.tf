locals {

  name_prefix = var.product.resource_name

  services = {
    for name, config in var.service_configs :
    name => config if !contains(["organisation-information-migrations", "organisation-app"], config.name)
  }

  endpoints = [
    for service in local.services :
    {
      name = service.name,
      url  = "https://${aws_api_gateway_domain_name.ecs_api.domain_name}/${service.name}/swagger/index.html"
    }
  ]

  orchestrator_account_id = var.account_ids["orchestrator"]

}
