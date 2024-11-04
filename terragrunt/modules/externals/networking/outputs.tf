
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

output "vpc_cider" {
  description = "ID of the VPC"
  value       = aws_vpc.this.cidr_block
}

output "vpc_cidr_block" {
  description = "VPC CIDR block"
  value       = aws_vpc.this.cidr_block
}

output "vpc_id" {
  description = "ID of the VPC"
  value       = aws_vpc.this.id
}
