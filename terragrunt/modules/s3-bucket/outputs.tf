output "arn" {
  value = aws_s3_bucket.this.arn
}

output "bucket" {
  value = aws_s3_bucket.this.bucket
}

output "id" {
  value = aws_s3_bucket.this.id
}

output "key_arn" {
  value = var.enable_encryption && var.sse_algorithm == "aws:kms" ? module.s3_kms_key[0].key_arn : null
}

output "key_id" {
  value = var.enable_encryption && var.sse_algorithm == "aws:kms" ? module.s3_kms_key[0].key_id : null
}
