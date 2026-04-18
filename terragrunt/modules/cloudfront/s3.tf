resource "aws_s3_bucket" "origin" {
  count  = var.cloudfront_manage_origin_bucket ? 1 : 0
  bucket = local.origin_bucket_name

  tags = merge(
    var.tags,
    {
      Name = local.origin_bucket_name
    }
  )
}

resource "aws_s3_bucket_ownership_controls" "origin" {
  count  = var.cloudfront_manage_origin_bucket ? 1 : 0
  bucket = aws_s3_bucket.origin[0].id

  rule {
    object_ownership = "BucketOwnerPreferred"
  }
}

resource "aws_s3_bucket_public_access_block" "origin" {
  count                   = var.cloudfront_manage_origin_bucket ? 1 : 0
  bucket                  = aws_s3_bucket.origin[0].id
  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_versioning" "origin" {
  count  = var.cloudfront_manage_origin_bucket ? 1 : 0
  bucket = aws_s3_bucket.origin[0].id

  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "origin" {
  count  = var.cloudfront_manage_origin_bucket ? 1 : 0
  bucket = aws_s3_bucket.origin[0].id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
    blocked_encryption_types = ["SSE-C"]
  }
}

resource "aws_s3_bucket_policy" "origin" {
  count  = var.cloudfront_manage_origin_bucket ? 1 : 0
  bucket = aws_s3_bucket.origin[0].id
  policy = data.aws_iam_policy_document.origin_bucket[0].json
}

resource "aws_s3_object" "index" {
  count = var.cloudfront_manage_origin_bucket && var.cloudfront_seed_origin ? 1 : 0

  bucket       = aws_s3_bucket.origin[0].id
  key          = "index.html"
  content_type = "text/html"
  content      = templatefile("${path.module}/assets/index.html", { environment = var.environment })
  etag         = md5(templatefile("${path.module}/assets/index.html", { environment = var.environment }))
}

resource "aws_s3_object" "error" {
  count = var.cloudfront_manage_origin_bucket && var.cloudfront_seed_origin ? 1 : 0

  bucket       = aws_s3_bucket.origin[0].id
  key          = "error.html"
  content_type = "text/html"
  source       = "${path.module}/assets/error.html"
  etag         = filemd5("${path.module}/assets/error.html")
}

resource "aws_s3_bucket" "logs" {
  bucket = local.log_bucket_name

  tags = merge(
    var.tags,
    {
      Name = local.log_bucket_name
    }
  )
}

resource "aws_s3_bucket_ownership_controls" "logs" {
  bucket = aws_s3_bucket.logs.id

  rule {
    object_ownership = "BucketOwnerPreferred"
  }
}

resource "aws_s3_bucket_public_access_block" "logs" {
  bucket                  = aws_s3_bucket.logs.id
  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_versioning" "logs" {
  bucket = aws_s3_bucket.logs.id

  versioning_configuration {
    status = "Suspended"
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "logs" {
  bucket = aws_s3_bucket.logs.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
    blocked_encryption_types = ["SSE-C"]
  }
}

resource "aws_s3_bucket_policy" "logs" {
  bucket = aws_s3_bucket.logs.id
  policy = data.aws_iam_policy_document.log_bucket.json
}

resource "aws_s3_bucket_lifecycle_configuration" "logs" {
  bucket = aws_s3_bucket.logs.id

  rule {
    id     = "expire-logs-90-days"
    status = "Enabled"

    expiration {
      days = 90
    }

    noncurrent_version_expiration {
      noncurrent_days = 90
    }
  }
}
