locals {
  name_prefix     = var.product.resource_name
  name_prefix_php = "${local.name_prefix}-php"

  tags = merge(var.tags, { Name = var.product.resource_name })

  waf_raw_ip_set_json = try(jsondecode(data.aws_secretsmanager_secret_version.waf_allowed_ips.secret_string), [])
  waf_allowed_ip_list_secret = length(local.waf_raw_ip_set_json) > 0 ? [
    for item in local.waf_raw_ip_set_json : item.value if can(item.value)
  ] : []

  waf_allowed_ip_list = concat(local.waf_allowed_ip_list_secret, [aws_vpc.this.cidr_block])

  waf_rule_sets_priority_blockers = {
    AWSManagedRulesAmazonIpReputationList : 4
    AWSManagedRulesKnownBadInputsRuleSet : 2
  }

  waf_rule_sets_priority_observers = {
    AWSManagedRulesSQLiRuleSet : 6
    AWSManagedRulesAnonymousIpList : 8
  }

  waf_raw_ip_set_json_tools = try(jsondecode(data.aws_secretsmanager_secret_version.waf_allowed_ips_tools.secret_string), [])
  waf_allowed_ip_list_tools_secret = length(local.waf_raw_ip_set_json_tools) > 0 ? [
    for item in local.waf_raw_ip_set_json_tools : item.value if can(item.value)
  ] : []

  waf_allowed_ip_list_tools = concat(local.waf_allowed_ip_list_tools_secret, [aws_vpc.this.cidr_block, "${aws_nat_gateway.this.public_ip}/32"])

}
