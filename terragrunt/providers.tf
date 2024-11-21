terraform {
  required_version = "= 1.9.5"
  required_providers {
    aws = {
      version = "~> 5.64.0"
      source  = "hashicorp/aws"
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
