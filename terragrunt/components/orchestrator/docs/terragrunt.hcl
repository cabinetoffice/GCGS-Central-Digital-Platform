terraform {
  source = local.global_vars.locals.environment == "orchestrator" ? "../../../modules//orchestrator/docs" : null
}

include {
  path = find_in_parent_folders("root.hcl")
}

locals {
  global_vars       = read_terragrunt_config(find_in_parent_folders("root.hcl"))
  orchestrator_vars = read_terragrunt_config(find_in_parent_folders("orchestrator.hcl"))

  tags = merge(
    local.global_vars.inputs.tags,
    local.orchestrator_vars.inputs.tags,
    {
      component = "orchestrator-docs"
    }
  )
}

inputs = {
  allowed_github_branches          = ["main", "previews/*", "staging"]
  bucket_name                      = "cdp-sirsi-documentations"
  cloudfront_acm_certificate_arn   = "arn:aws:acm:us-east-1:891377225335:certificate/fff79771-ec19-4efd-bb14-9a010e626a6d"
  cloudfront_custom_domain_enabled = true
  cloudfront_enabled               = true
  cloudfront_price_class           = "PriceClass_100"
  docs_domain_name                 = "docs.supplier-information.find-tender.service.gov.uk"
  enable_encryption                = true
  github_org                       = "cabinetoffice"
  github_repo                      = "gcgs-cdp-documentation"
  is_public                        = true
  product                          = local.global_vars.locals.product
  sse_algorithm                    = "AES256"
  tags                             = local.tags
}
