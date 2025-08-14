resource "aws_s3_bucket" "this" {
  bucket = var.bucket_name

  tags = merge(
    var.tags,
    {
      Name = var.bucket_name
    }
  )
}


resource "aws_s3_bucket_website_configuration" "this" {
  count  = var.is_public ? 1 : 0
  bucket = aws_s3_bucket.this.id

  index_document {
    suffix = "index.html"
  }

  error_document {
    key = "error.html"
  }
}

resource "aws_s3_bucket_ownership_controls" "bucket" {
  bucket = aws_s3_bucket.this.id

  rule {
    object_ownership = "BucketOwnerPreferred"
  }
}

resource "aws_s3_bucket_public_access_block" "this" {
  block_public_acls       = !var.is_public
  block_public_policy     = !var.is_public
  bucket                  = aws_s3_bucket.this.id
  ignore_public_acls      = !var.is_public
  restrict_public_buckets = !var.is_public
}

resource "aws_s3_bucket_policy" "private" {
  count = var.is_public ? 0 : 1

  bucket     = aws_s3_bucket.this.id
  depends_on = [aws_s3_bucket_public_access_block.this]
  policy     = data.aws_iam_policy_document.private.json
}

resource "aws_s3_bucket_policy" "public" {
  count = var.is_public ? 1 : 0

  bucket     = aws_s3_bucket.this.id
  depends_on = [aws_s3_bucket_public_access_block.this]
  policy     = data.aws_iam_policy_document.public.json
}

resource "aws_s3_bucket_versioning" "bucket" {
  bucket = aws_s3_bucket.this.id

  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "encrypted_bucket" {
  count = var.enable_encryption ? 1 : 0

  bucket = aws_s3_bucket.this.id

  rule {
    apply_server_side_encryption_by_default {
      kms_master_key_id = module.s3_kms_key.key_arn
      sse_algorithm     = "aws:kms"
    }
    bucket_key_enabled = true
  }
}

resource "aws_s3_bucket" "log_bucket" {
  count  = var.enable_access_logging ? 1 : 0
  bucket = "${var.bucket_name}-logs"

  tags = {
    Name = "${var.bucket_name}-logs"
  }
}

resource "aws_s3_bucket_public_access_block" "log_bucket" {
  count                   = var.enable_access_logging ? 1 : 0
  bucket                  = aws_s3_bucket.log_bucket[0].id
  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_server_side_encryption_configuration" "log_bucket" {
  count  = var.enable_access_logging ? 1 : 0
  bucket = aws_s3_bucket.log_bucket[0].id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
  }
}

resource "aws_s3_bucket_logging" "this" {
  count         = var.enable_access_logging ? 1 : 0
  bucket        = aws_s3_bucket.this.id
  target_bucket = aws_s3_bucket.log_bucket[0].id
  target_prefix = "${var.bucket_name}/logs/"
}
