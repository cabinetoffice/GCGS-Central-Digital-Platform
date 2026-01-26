output "dashboard_endpoint" {
  value       = length(aws_opensearch_domain.this) > 0 ? aws_opensearch_domain.this[0].dashboard_endpoint : null
  description = "Dashboards endpoint (VPC reachable)."
}

output "domain_arn" {
  value       = length(aws_opensearch_domain.this) > 0 ? aws_opensearch_domain.this[0].arn : null
  description = "OpenSearch domain ARN."
}

output "domain_name" {
  value       = length(aws_opensearch_domain.this) > 0 ? aws_opensearch_domain.this[0].domain_name : null
  description = "OpenSearch domain name."
}

output "endpoint" {
  value       = length(aws_opensearch_domain.this) > 0 ? aws_opensearch_domain.this[0].endpoint : null
  description = "Domain endpoint (VPC reachable)."
}
