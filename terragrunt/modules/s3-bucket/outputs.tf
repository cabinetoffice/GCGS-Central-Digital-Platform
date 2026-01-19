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
  value = module.s3_kms_key.key_arn
}

output "key_id" {
  value = module.s3_kms_key.key_id
}
