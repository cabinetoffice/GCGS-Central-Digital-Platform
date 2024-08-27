terraform {
  required_version = "= 1.9.5"
  required_providers {
    aws = {
      version = "~> 5.64.0"
      source  = "hashicorp/aws"
    }
  }
}

provider "aws" {
  region = "eu-west-2"
}
