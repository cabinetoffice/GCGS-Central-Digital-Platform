locals {

  name_prefix = var.product.resource_name

  read_principals = [for name, id in var.account_ids : "arn:aws:iam::${id}:root"]


  service_repositories = [
    for name, config in var.service_configs :
    config.name
  ]

  tools_repositories = concat([
    for name, config in var.tools_configs :
    config.name if !contains(["opensearch_admin", "opensearch_gateway"], name)
  ], ["codebuild"])

  repositories = concat(local.service_repositories, local.tools_repositories)


}
