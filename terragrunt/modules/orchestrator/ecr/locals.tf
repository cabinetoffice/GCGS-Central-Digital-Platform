locals {

  name_prefix = var.product.resource_name

  read_principals = [for name, id in var.account_ids : "arn:aws:iam::${id}:root"]

  tools_repositories = ["grafana", "codebuild", "pgadmin", "healthcheck"]

  repositories = concat(
    [
      for name, config in var.service_configs :
      config.name
    ],
  local.tools_repositories)

}
