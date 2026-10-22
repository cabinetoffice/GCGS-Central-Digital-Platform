data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_codestarconnections_connection" "cabinet_office" {
  name = "CabinetOffice"
}
