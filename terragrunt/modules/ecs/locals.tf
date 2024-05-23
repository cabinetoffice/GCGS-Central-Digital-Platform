locals {

  ecr_urls = [
    for repos in aws_ecr_repository.this.* : { for repo, attr in repos : repo => attr.repository_url }
    ][
    0
  ]

  name_prefix = var.product.resource_name

  one_loging = {
    credential_locations = {
      authority   = "${data.aws_secretsmanager_secret.one_login.arn}:Authority::"
      client_id   = "${data.aws_secretsmanager_secret.one_login.arn}:ClientId::"
      private_key = "${data.aws_secretsmanager_secret.one_login.arn}:PrivateKey::"
    }
  }

  services = [
    "data-sharing",
    "forms",
    "organisation",
    "organisation-app",
    "person",
    "tenant"
  ]

  tasks = concat(local.services, ["organisation-information-migrations"])

}
