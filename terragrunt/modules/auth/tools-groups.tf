resource "aws_cognito_user_group" "tools_s3_uploader" {
  name         = "tools-s3-uploader"
  description  = "Access to the s3-uploader tool"
  user_pool_id = aws_cognito_user_pool.tools.id
}
