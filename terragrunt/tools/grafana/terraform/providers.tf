terraform {
  required_version = ">= 1.3.0"

  required_providers {
    grafana = {
      source  = "grafana/grafana"
      version = "~> 4.35"
    }
  }
}

provider "grafana" {
  url  = var.grafana_url
  auth = var.grafana_token
}
