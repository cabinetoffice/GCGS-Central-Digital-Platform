locals {

  name_prefix = var.product.resource_name

  service_port_map = {
    data-sharing = 8088
    forms        = 8086
    organisation = 8082
    person       = 8084
    tenant       = 8080
  }

}
