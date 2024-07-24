locals {

  name_prefix = var.product.resource_name

  bootstrap_roles = [
    for name, id in var.account_ids : "arn:aws:iam::${id}:role/bootstrap"
  ]

  codebuild_roles = [
    for name, id in var.account_ids : "arn:aws:iam::${id}:role/${local.name_prefix}-${name}-ci-codebuild"
  ]


  terraform_roles = [
    for name, id in var.account_ids : "arn:aws:iam::${id}:role/${local.name_prefix}-${name}-terraform"
  ]

  combined_roles = concat(local.terraform_roles, local.codebuild_roles, local.bootstrap_roles)

}
