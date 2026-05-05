terraform {
  required_version = "= 1.15.1"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.43.0"
    }
    awscc = {
      source  = "hashicorp/awscc"
      version = "1.82.0"
    }
  }
}

provider "aws" {
  region = "eu-west-2"
}
