resource "aws_cognito_user_group" "tools_s3_uploader" {
  name         = "tools-s3-uploader"
  description  = "Access to the s3-uploader tool"
  user_pool_id = aws_cognito_user_pool.tools.id
}

resource "aws_cognito_user_group" "tools_filestash" {
  name         = "tools-filestash"
  description  = "Access to the e2e-reports (Filestash) tool"
  user_pool_id = aws_cognito_user_pool.tools.id
}
