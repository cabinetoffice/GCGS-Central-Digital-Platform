terraform {
  required_version = "= 1.9.2"
  required_providers {
    aws = {
      version = "~> 5.48.0"
      source  = "hashicorp/aws"
    }
  }
}

provider "aws" {
  region = "eu-west-2"
}
