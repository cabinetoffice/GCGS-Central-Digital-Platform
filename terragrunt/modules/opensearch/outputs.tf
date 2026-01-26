output "dashboard_endpoint" {
  value       = aws_opensearch_domain.this[0].dashboard_endpoint
  description = "Dashboards endpoint (VPC reachable)."
}

output "domain_arn" {
  value       = aws_opensearch_domain.this[0].arn
  description = "OpenSearch domain ARN."
}

output "domain_name" {
  value       = aws_opensearch_domain.this[0].domain_name
  description = "OpenSearch domain name."
}

output "endpoint" {
  value       = aws_opensearch_domain.this[0].endpoint
  description = "Domain endpoint (VPC reachable)."
}
