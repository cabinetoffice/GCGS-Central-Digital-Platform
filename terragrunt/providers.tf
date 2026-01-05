terraform {
  required_version = "= 1.14.3"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.27.0"
    }
    awscc = {
      source  = "hashicorp/awscc"
      version = "1.67.0"
    }
  }
}

provider "aws" {
  region = "eu-west-2"
}
