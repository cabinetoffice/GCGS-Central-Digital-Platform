variable "delay_seconds" {
  description = "The time in seconds that the delivery of all messages in the queue will be delayed."
  type        = number
  default     = 0
}

variable "max_message_size" {
  description = "The limit of how many bytes a message can contain before Amazon SQS rejects it."
  type        = number
  default     = 262144 # 256 KiB
}

variable "max_receive_count" {
  default     = 10
  description = ""
  type        = number
}

variable "message_retention_seconds" {
  description = "The number of seconds Amazon SQS retains a message."
  type        = number
  default     = 345600 # 4 Days
}

variable "name" {
  description = "Queues name"
  type        = string
}

variable "receive_wait_time_seconds" {
  description = "The time for which a ReceiveMessage call will wait for a message to arrive."
  type        = number
  default     = 0
}

variable "role_consumer_arn" {
  description = "List of queue's consumer IAM role ARNs"
  type        = list(string)
}

variable "role_publisher_arn" {
  description = "List of queue's publisher IAM role ARNs"
  type        = list(string)
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}

variable "visibility_timeout_seconds" {
  description = "The visibility timeout for the queue."
  type        = number
  default     = 30
}
