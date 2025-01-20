variable "fts_azure_frontdoor" {
  description = "The DNS hostname to used for routing traffic to the Find a Tender Service (FTS)"
  type        = string
  default     = null
}

variable "hosted_zone_id" {
  description = "ID of the hosted zone to deploy to"
  type        = string
}

