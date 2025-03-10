locals {
  name_prefix = var.product.resource_name

  production_subdomain = "supplier-information"

  tags = merge(var.tags, { Name = var.product.resource_name })

  waf_allowed_ip_list = length(local.waf_raw_ip_set_json) > 0 ? [
    for item in local.waf_raw_ip_set_json : item.value if can(item.value)
  ] : []
  waf_raw_ip_set_json = try(jsondecode(data.aws_secretsmanager_secret_version.waf_allowed_ips.secret_string), [])

  waf_rule_sets_priority_blockers = {
    AWSManagedRulesAmazonIpReputationList : 4
    AWSManagedRulesKnownBadInputsRuleSet : 2
  }

  waf_rule_sets_priority_observers = {
    AWSManagedRulesSQLiRuleSet : 6
    AWSManagedRulesAnonymousIpList : 8
  }

}
