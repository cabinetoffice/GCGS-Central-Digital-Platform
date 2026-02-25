module "docs_bucket" {
  source = "../../s3-bucket"

  bucket_name           = local.bucket_name
  create_kms_key        = false
  enable_access_logging = var.enable_access_logging
  enable_encryption     = var.enable_encryption
  is_public             = var.is_public
  sse_algorithm         = var.sse_algorithm
  tags                  = var.tags
}

resource "aws_iam_openid_connect_provider" "github" {
  url             = "https://token.actions.githubusercontent.com"
  client_id_list  = ["sts.amazonaws.com"]
  thumbprint_list = var.github_oidc_thumbprints
  tags            = var.tags
}
