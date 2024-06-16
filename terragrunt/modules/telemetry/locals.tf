locals {
  name_prefix = var.product.resource_name

  service_widgets = [
    for idx, service in values(var.service_configs) : [
      {
        name    = service.name
        type    = idx % 2 == 0 ? "warn" : "err"
        x       = 12 * (idx % 2)
        y       = 6 * floor(idx / 2)
        width   = 12
        height  = 6
      },
      {
        name    = service.name
        type    = idx % 2 != 0 ? "warn" : "err"
        x       = 12 * ((idx + 1) % 2)
        y       = 6 * floor((idx + 1) / 2)
        width   = 12
        height  = 6
      }
    ]
  ]

  flat_service_widgets = flatten(local.service_widgets)

}
