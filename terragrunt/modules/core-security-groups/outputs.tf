output "canary_sg_id" {
  value = aws_security_group.canary.id
}

output "ci_sg_id" {
  value = aws_security_group.ci.id
}

output "db_mysql_sg_id" {
  value = aws_security_group.db_mysql.id
}

output "db_postgres_sg_id" {
  value = aws_security_group.db_postgres.id
}

output "ecs_sg_id" {
  value = aws_security_group.ecs.id
}

output "elasticache_redis_sg_id" {
  value = aws_security_group.elasticache_redis.id
}

output "vpce_ecr_api_sg_id" {
  value = aws_security_group.vpce_ecr_api.id
}

output "vpce_ecr_dkr_sg_id" {
  value = aws_security_group.vpce_ecr_dkr.id
}

output "vpce_logs_sg_id" {
  value = aws_security_group.vpce_logs.id
}

output "vpce_s3_sg_id" {
  value = aws_security_group.vpce_s3.id
}

output "vpce_secretsmanager_sg_id" {
  value = aws_security_group.vpce_secretsmanager.id
}
