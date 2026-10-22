variable "account_ids" {
  description = "Map of environment name -> AWS account id"
  type        = map(string)
}

variable "ci_build_role_arn" {
  type = string
}

variable "ci_pipeline_role_arn" {
  type = string
}

variable "ci_sg_id" {
  type = string
}

variable "environment" {
  type = string
}

variable "private_subnet_ids" {
  type = list(string)
}

variable "product" {
  type = object({
    name               = string
    resource_name      = string
    public_hosted_zone = string
  })
}

variable "repository_urls" {
  type = map(string)
}

variable "role_terraform_arn" {
  type = string
}

variable "schedule_timezone" {
  type        = string
  default     = "Europe/London"
  description = "Timezone used for the weekly schedule"
}

variable "tags" {
  type = map(string)
}

variable "vpc_id" {
  type = string
}

variable "fts_refresh_restore_mode" {
  type        = string
  default     = "main"
  description = "Controls how staging restore is performed. main = restore the real staging FTS cluster via Terragrunt apply; scratch = restore into a separate test cluster and run post-restore SQL without impacting staging services."

  validation {
    condition     = contains(["main", "scratch"], var.fts_refresh_restore_mode)
    error_message = "fts_refresh_restore_mode must be one of: main, scratch"
  }
}

variable "keep_scratch" {
  type        = bool
  default     = true
  description = "If true and fts_refresh_restore_mode=scratch, leave the scratch cluster in place after the pipeline run (recommended during testing)."
}

variable "fts_refresh_delete_main_cluster" {
  type        = bool
  default     = true
  description = "If true and fts_refresh_restore_mode=main, delete the existing staging FTS cluster before restoring from the production snapshot (so the restored cluster keeps the same identifier)."
}
