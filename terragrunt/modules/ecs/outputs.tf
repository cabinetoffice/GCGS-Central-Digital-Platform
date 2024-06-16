output "certificate_arn" {
  value = aws_acm_certificate.this.arn
}

output "ecs_cluster_id" {
  value = aws_ecs_cluster.this.id
}

output "ecs_lb_dns_name" {
  value = aws_lb.ecs.dns_name
}

output "ecs_listener_arn" {
  value = aws_lb_listener.ecs.arn
}
