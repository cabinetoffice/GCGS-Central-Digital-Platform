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

dependency core_iam {
  config_path = "../../core/iam"
  mock_outputs = {
    cloudfront_realtime_logs_role_arn = "mock"
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
  cloudfront_custom_domain_enabled  = true
  cloudfront_aliases                = ["cdn.${trimsuffix(dependency.core_networking.outputs.public_domain, ".")}"]
  cloudfront_default_root_object    = "index.html"
  cloudfront_realtime_logs_role_arn = dependency.core_iam.outputs.cloudfront_realtime_logs_role_arn
  cloudfront_manage_route53         = true
  cloudfront_route53_zone_id        = dependency.core_networking.outputs.public_hosted_zone_id
  tags                              = local.tags
}
