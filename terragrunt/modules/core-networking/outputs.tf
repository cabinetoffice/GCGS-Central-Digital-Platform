output "nat_gateway_id" {
  value = aws_nat_gateway.this.id
}

output "private_route_table_ids" {
  value = aws_route_table.private.*.id
}

output "private_subnet_arns" {
  description = "VPC private subnet ARNs"
  value       = aws_subnet.private.*.id
}

output "private_subnet_ids" {
  description = "VPC private subnet IDs"
  value       = aws_subnet.private.*.id
}

output "private_subnets_cidr_blocks" {
  description = "VPC private subnet CIDR blocks"
  value       = aws_subnet.private.*.cidr_block
}

output "public_hosted_zone_fqdn" {
  value = aws_route53_zone.public.name
}

output "public_hosted_zone_id" {
  value = aws_route53_zone.public.id
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

output "vpc_cidr_block" {
  description = "VPC CIDR block"
  value       = aws_vpc.this.cidr_block
}

output "vpc_id" {
  description = "ID of the VPC"
  value       = aws_vpc.this.id
}
