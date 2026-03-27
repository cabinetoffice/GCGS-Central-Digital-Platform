resource "aws_s3_bucket" "origin" {
  bucket = local.origin_bucket_name

  tags = merge(
    var.tags,
    {
      Name = local.origin_bucket_name
    }
  )
}

resource "aws_s3_bucket_ownership_controls" "origin" {
  bucket = aws_s3_bucket.origin.id

  rule {
    object_ownership = "BucketOwnerPreferred"
  }
}

resource "aws_s3_bucket_public_access_block" "origin" {
  bucket                  = aws_s3_bucket.origin.id
  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_versioning" "origin" {
  bucket = aws_s3_bucket.origin.id

  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "origin" {
  bucket = aws_s3_bucket.origin.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
  }
}

resource "aws_s3_bucket_policy" "origin" {
  bucket = aws_s3_bucket.origin.id
  policy = data.aws_iam_policy_document.origin_bucket.json
}

resource "aws_s3_object" "index" {
  count = var.cloudfront_seed_origin ? 1 : 0

  bucket       = aws_s3_bucket.origin.id
  key          = "index.html"
  content_type = "text/html"
  content      = templatefile("${path.module}/assets/index.html", { environment = var.environment })
  etag         = md5(templatefile("${path.module}/assets/index.html", { environment = var.environment }))
}

resource "aws_s3_object" "error" {
  count = var.cloudfront_seed_origin ? 1 : 0

  bucket       = aws_s3_bucket.origin.id
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
    status = "Enabled"
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "logs" {
  bucket = aws_s3_bucket.logs.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
  }
}

resource "aws_s3_bucket_policy" "logs" {
  bucket = aws_s3_bucket.logs.id
  policy = data.aws_iam_policy_document.log_bucket.json
}
