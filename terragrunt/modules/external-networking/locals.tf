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

  payments_records = {
    oechestrator = {}
    development = {}
    staging     = {
      payments = {
        name              = "payments"
        records           = ["cdp-payments-alb-staging-1243004017.eu-west-2.elb.amazonaws.com"]
        lb_zone_id        = "ZHURV8PSTC4K8"
        validator_name    = "_52736daa2f84679208a08c42f427e677.payments.staging.supplier-information.find-tender.service.gov.uk."
        validator_records = ["_aa597b611a8bfdf011f6b2328f946e8f.jkddzztszm.acm-validations.aws."]
      }
    }
    integration = {}
    production = {
      publish_payments = {
        name              = "publish-payments"
        records           = ["cdp-payments-alb-prod-764539923.eu-west-2.elb.amazonaws.com"]
        lb_zone_id        = "ZHURV8PSTC4K8"
        validator_name    = "_b31383e3f63455a38308c3fd1b5ae823.publish-payments.supplier-information.find-tender.service.gov.uk."
        validator_records = ["_0689d01b5b6e22864b5a788204e3a81c.jkddzztszm.acm-validations.aws."]
      }
      payments_api = {
        name              = "payments-api"
        records           = ["cdp-payments-alb-prod-764539923.eu-west-2.elb.amazonaws.com"]
        lb_zone_id        = "ZHURV8PSTC4K8"
        validator_name    = "_6ddecfa4d447c2cb8e93f33a3f5accad.payments-api.supplier-information.find-tender.service.gov.uk."
        validator_records = ["_7628fb71a53e0f4a2b4cd2e81a106250.jkddzztszm.acm-validations.aws."]
      }
      payments_transparency_register = {
        name              = "payments-transparency-register"
        records           = ["cdp-payments-alb-prod-764539923.eu-west-2.elb.amazonaws.com"]
        lb_zone_id        = "ZHURV8PSTC4K8"
        validator_name    = "_5b06e0b6511ee7d227fd18c99fcd851a.payments-transparency-register.supplier-information.find-tender.service.gov.uk."
        validator_records = ["_20c8d7ccf1b9c7ed45bcc82a1c74ebca.jkddzztszm.acm-validations.aws."]
      }
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
