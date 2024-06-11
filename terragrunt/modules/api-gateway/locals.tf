locals {

  name_prefix = var.product.resource_name

  services = {
    for name, config in var.service_configs :
    name => config if !contains(["organisation-information-migrations", "organisation-app"], config.name)
  }

}
