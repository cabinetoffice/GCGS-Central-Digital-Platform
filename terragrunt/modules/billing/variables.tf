variable "tags" {
  description = "A map of tags to add to all resources"
  type        = map(string)
  default     = {}
}

variable "cur_bucket_name" {
  description = "Name of the S3 bucket to store Cost and Usage Reports"
  type        = string
}

variable "budget_amount" {
  description = "The amount of cost or usage being measured for a budget"
  type        = string
  default     = "1000"
}

variable "budget_alert_emails" {
  description = "List of email addresses to send budget alerts to"
  type        = list(string)
  default     = []
}
