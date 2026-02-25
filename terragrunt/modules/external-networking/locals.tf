locals {

  commercial_ai_records = {
    oechestrator = null
    development = {
      name              = "ai"
      records           = ["cdp-daps-cai-ecs-394200431.eu-west-2.elb.amazonaws.com"]
      validator_name    = "_2fd0fb3da612e417e712cc6c4e3ce3eb.ai.dev.supplier-information.find-tender.service.gov.uk."
      validator_records = ["_f7e371359d8616d65633cce25d1ac449.jkddzztszm.acm-validations.aws."]
    }
    staging     = null
    integration = null
    production = {
      name              = "commercial-ai"
      records           = ["wdq6jpemgg.eu-west-2.awsapprunner.com"]
      validator_name    = "_7fe1a72107ce559d9bf4cd43164da019"
      validator_records = ["_1616d9f5d97e70042aafc1b4ecb98fab.xlfgrmvvlj.acm-validations.aws."]
    }
  }

  docs_records = {
    oechestrator = null
    development  = null
    staging      = null
    integration  = null
    production = {
      name              = "docs"
      records           = ["d3ilb273zh778b.cloudfront.net"]
      validator_name    = "_6503e13207edacb70a31e80c16cfe3da.docs.supplier-information.find-tender.service.gov.uk."
      validator_records = ["_f6b5f10281167eb861a98afc996f9afd.jkddzztszm.acm-validations.aws."]
    }
  }
}
