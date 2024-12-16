locals {
  name_prefix = var.product.resource_name

  production_subdomain = "supplier-information"

  tags = merge(var.tags, { Name = var.product.resource_name })

  waf_rule_sets_priority = {
    AWSManagedRulesAmazonIpReputationList : 2
    AWSManagedRulesAnonymousIpList : 3
    # AWSManagedRulesBotControlRuleSet : 5
    # AWSManagedRulesCommonRuleSet : 1
    AWSManagedRulesSQLiRuleSet : 4
  }
}
