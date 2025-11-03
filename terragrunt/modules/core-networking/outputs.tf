output "nat_gateway_id" {
  value = aws_nat_gateway.this.id
}

output "private_beta_domain" {
  value = try(aws_route53_zone.production_private_beta[0].name, null)
}

output "private_beta_hosted_zone_id" {
  value = try(aws_route53_zone.production_private_beta[0].id, null)
}

output "private_route_table_ids" {
  value = aws_route_table.private.*.id
}

output "private_subnet_arns" {
  description = "VPC private subnet ARNs"
  value       = aws_subnet.private.*.arn
}

output "private_subnet_ids" {
  description = "VPC private subnet IDs"
  value       = aws_subnet.private.*.id
}

output "private_subnets_cidr_blocks" {
  description = "VPC private subnet CIDR blocks"
  value       = aws_subnet.private.*.cidr_block
}

output "public_domain" {
  value = aws_route53_zone.public.name
}

output "public_hosted_zone_fqdn" {
  value = aws_route53_zone.public.name
}

output "public_hosted_zone_id" {
  value = aws_route53_zone.public.id
}

output "public_hosted_zone_cfs_fqdn" {
  value = try(aws_route53_zone.cfs[0].name, null)
}

output "public_hosted_zone_cfs_id" {
  value = try(aws_route53_zone.cfs[0].id, null)
}

output "public_hosted_zone_fts_fqdn" {
  value = try(aws_route53_zone.fts[0].name, null)
}

output "public_hosted_zone_fts_id" {
  value = try(aws_route53_zone.fts[0].id, null)
}

output "public_route_table_ids" {
  value = aws_route_table.private.*.id
}

output "public_subnet_ids" {
  description = "VPC public subnet IDs"
  value       = aws_subnet.public.*.id
}

output "public_subnets_cidr_blocks" {
  description = "Public subnet CIDR blocks"
  value       = aws_subnet.public.*.cidr_block
}

output "vpc_cider" {
  description = "ID of the VPC"
  value       = aws_vpc.this.cidr_block
}

output "vpc_cidr_blocks" {
  description = "VPC CIDR blocks"
  value       = [aws_vpc.this.cidr_block]
}

output "vpc_id" {
  description = "ID of the VPC"
  value       = aws_vpc.this.id
}

output "waf_acl_arn" {
  value = try(aws_wafv2_web_acl.this[0].arn, null)
}

output "waf_acl_php_arn" {
  value = try(aws_wafv2_web_acl.php[0].arn, null)
}

output "waf_acl_tools_arn" {
  value = try(aws_wafv2_web_acl.tools[0].arn, null)
}
