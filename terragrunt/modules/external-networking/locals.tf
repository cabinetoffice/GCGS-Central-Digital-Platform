locals {

  cloudfront_global_hosted_zone_id = "Z2FDTNDATAQYW2"

  commercial_ai_cloudfront_domain = "d1kry5jfpfm6f7.cloudfront.net"

  commercial_ai_cert_validator_records = {
    oechestrator = null
    development  = null
    staging      = null
    integration  = null
    production = {
      name    = "_7fe1a72107ce559d9bf4cd43164da019"
      records = ["_1616d9f5d97e70042aafc1b4ecb98fab.xlfgrmvvlj.acm-validations.aws."]
    }
  }
}
