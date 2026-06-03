terraform {
  required_version = ">= 1.3.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
    grafana = {
      source  = "grafana/grafana"
      version = "4.35.0"
    }
  }
}

provider "aws" {
  region = "eu-west-2"
}

provider "grafana" {
  url  = var.grafana_url
  auth = var.grafana_token
}
