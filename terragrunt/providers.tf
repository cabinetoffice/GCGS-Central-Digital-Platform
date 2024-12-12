terraform {
  required_version = "= 1.10.2"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.80.0"
    }
    awscc = {
      source  = "hashicorp/awscc"
      version = "1.2.0"
    }
  }
}

provider "aws" {
  region = "eu-west-2"
}
