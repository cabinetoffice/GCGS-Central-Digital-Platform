variable "fts_azure_frontdoor" {
  description = "The DNS hostname to used for routing traffic to the Find a Tender Service (FTS)"
  type        = string
  default     = null
}

variable "public_hosted_zone_id" {
  description = "ID of the public hosted zone"
  type        = string
}

