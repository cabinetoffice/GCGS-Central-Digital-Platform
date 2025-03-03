output "port" {
  value = aws_elasticache_replication_group.this.port
}

output "primary_endpoint_address" {
  value = aws_elasticache_replication_group.this.primary_endpoint_address
}

output "reader_endpoint_address" {
  value = aws_elasticache_replication_group.this.reader_endpoint_address
}

output "redis_auth_token_arn" {
  value = aws_secretsmanager_secret.redis_auth_token.arn
}

output "redis_auth_token_id" {
  value = aws_secretsmanager_secret.redis_auth_token.id
}

output "redis_cluster_node_ids" {
  value = [
    for i in range(1, aws_elasticache_replication_group.this.num_cache_clusters + 1) :
    "${aws_elasticache_replication_group.this.id}-${format("%03d", i)}"
  ]
}
