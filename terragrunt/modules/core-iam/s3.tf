resource "aws_s3_bucket_policy" "tfstate" {
  bucket = data.aws_s3_bucket.tfstate.bucket
  policy = data.aws_iam_policy_document.tfstate.json
}
