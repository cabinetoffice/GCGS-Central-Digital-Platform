variable "grafana_url" {
  description = "Base URL for Grafana (e.g. https://grafana.<domain>)"
  type        = string
}

variable "grafana_token" {
  description = "Grafana API token"
  type        = string
  sensitive   = true
}

variable "teams_webhook_url" {
  description = "Microsoft Teams webhook URL"
  type        = string
  sensitive   = true
  default     = ""
}

variable "alert_contact_point_name" {
  description = "Name for the Teams contact point"
  type        = string
  default     = "Microsoft Teams"
}
