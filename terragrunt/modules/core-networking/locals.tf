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

  waf_allowed_ip_list_tools = concat(
    local.waf_allowed_ip_list_tools_secret,
    [aws_vpc.this.cidr_block, "${aws_nat_gateway.this.public_ip}/32"]
  )

  hosted_zones = {
    cfs = {
      development = "www-development.contractsfinder.service.gov.uk"
      staging     = "www-preview.contractsfinder.service.gov.uk"
      integration = "www-integration.contractsfinder.service.gov.uk"
      production  = "contractsfinder.service.gov.uk"
    }

    fts = {
      development = "www-development.find-tender.service.gov.uk"
      staging     = "www-staging.find-tender.service.gov.uk"
      integration = "www-tpp.find-tender.service.gov.uk"
      production  = "find-tender.service.gov.uk"
    }
  }

  delegation_ns_records = {
    cfs = {
      "www-development.contractsfinder.service.gov.uk" = [
        "ns-1300.awsdns-34.org.",
        "ns-1927.awsdns-48.co.uk.",
        "ns-713.awsdns-25.net.",
        "ns-467.awsdns-58.com.",
      ],
      "www-preview.contractsfinder.service.gov.uk" = [
        "ns-443.awsdns-55.com.",
        "ns-1438.awsdns-51.org.",
        "ns-850.awsdns-42.net.",
        "ns-1668.awsdns-16.co.uk.",
      ],
      "www-integration.contractsfinder.service.gov.uk" = [
        "ns-1416.awsdns-49.org.",
        "ns-1687.awsdns-18.co.uk.",
        "ns-473.awsdns-59.com.",
        "ns-861.awsdns-43.net.",
      ],
    }

    fts = {
      "www-development.find-tender.service.gov.uk" = [
        "ns-889.awsdns-47.net.",
        "ns-1463.awsdns-54.org.",
        "ns-40.awsdns-05.com.",
        "ns-1839.awsdns-37.co.uk.",
      ],
      "www-staging.find-tender.service.gov.uk" = [
        "ns-144.awsdns-18.com.",
        "ns-1268.awsdns-30.org.",
        "ns-677.awsdns-20.net.",
        "ns-2044.awsdns-63.co.uk.",
      ],
      "www-tpp.find-tender.service.gov.uk" = [
        "ns-778.awsdns-33.net.",
        "ns-1272.awsdns-31.org.",
        "ns-2006.awsdns-58.co.uk.",
        "ns-509.awsdns-63.com.",
      ],
      "supplier-information.find-tender.service.gov.uk" = [
        "ns-1446.awsdns-52.org.",
        "ns-90.awsdns-11.com.",
        "ns-942.awsdns-53.net.",
        "ns-1925.awsdns-48.co.uk.",
      ],
    }

    sirsi = {
      "dev.supplier-information.find-tender.service.gov.uk" = [
        "ns-318.awsdns-39.com.",
        "ns-1119.awsdns-11.org.",
        "ns-814.awsdns-37.net.",
        "ns-1696.awsdns-20.co.uk.",
      ],
      "staging.supplier-information.find-tender.service.gov.uk" = [
        "ns-613.awsdns-12.net.",
        "ns-1329.awsdns-38.org.",
        "ns-482.awsdns-60.com.",
        "ns-1871.awsdns-41.co.uk.",
      ],
      "integration.supplier-information.find-tender.service.gov.uk" = [
        "ns-559.awsdns-05.net.",
        "ns-57.awsdns-07.com.",
        "ns-1067.awsdns-05.org.",
        "ns-1636.awsdns-12.co.uk.",
      ],
    }
  }
}
