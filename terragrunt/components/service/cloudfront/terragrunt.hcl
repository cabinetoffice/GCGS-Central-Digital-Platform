terraform {
  source = local.global_vars.locals.environment != "orchestrator" ? "../../../modules//cloudfront" : null
}

include {
  path = find_in_parent_folders("root.hcl")
}

dependency core_networking {
  config_path = "../../core/networking"
  mock_outputs = {
    public_domain         = "mock"
    public_hosted_zone_id = "mock"
  }
}

locals {
  global_vars  = read_terragrunt_config(find_in_parent_folders("root.hcl"))
  service_vars = read_terragrunt_config(find_in_parent_folders("service.hcl"))

  tags = merge(
    local.global_vars.inputs.tags,
    local.service_vars.inputs.tags,
    {
      component = "cloudfront"
    }
  )
}

inputs = {
  cloudfront_custom_domain_enabled = true
  cloudfront_enabled               = true
  cloudfront_price_class           = "PriceClass_100"
  cloudfront_aliases               = ["cdn.${trimsuffix(dependency.core_networking.outputs.public_domain, ".")}"]
  cloudfront_default_root_object   = "index.html"
  cloudfront_logging_enabled       = true
  cloudfront_response_headers_policy_enabled = true
  cloudfront_seed_origin           = true
  cloudfront_manage_route53        = true
  cloudfront_route53_zone_id       = dependency.core_networking.outputs.public_hosted_zone_id
  waf_enabled                      = true
  waf_bot_control_enabled          = true
  waf_logging_enabled              = true
  tags                             = local.tags
}
