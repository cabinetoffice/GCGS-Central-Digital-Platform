locals {
  terraform_roles = [
    for name, id in var.account_ids : "arn:aws:iam::${id}:role/cdp-sirsi-${name}-terraform"
  ]

  codebuild_roles = [
    for name, id in var.account_ids : "arn:aws:iam::${id}:role/cdp-sirsi-${name}-ci-codebuild"
  ]

  combined_roles = concat(local.terraform_roles, local.codebuild_roles)
}
