variable "aurora_mysql_engine_version" {
  description = "DB engine version"
  type        = string
}

variable "aurora_mysql_family" {
  description = "DB family"
  type        = string
}

variable "aurora_mysql_instance_type" {
  description = "FTS RDS instance type"
  type        = string
  default     = "db.r5.large"
}

variable "aurora_postgres_engine_version" {
  description = "DB engine version"
  type        = string
  default     = "16.6"
}

variable "aurora_postgres_instance_type" {
  description = "Sirsi's (a.k.a Organisation) RDS instance type"
  type        = string
  default     = "db.r5.large"
}

variable "aurora_postgres_instance_type_ev" {
  description = "Entity Verification's RDS instance type"
  type        = string
  default     = "db.r5.large"
}

variable "db_mysql_sg_id" {
  description = "MySQL DB security group ID"
  type        = string
}

variable "db_postgres_sg_id" {
  description = "Postgres DB security group ID"
  type        = string
}

variable "ec2_sg_id" {
  description = "EC2 instance for DB migration security group ID"
  type        = string
}

variable "environment" {
  description = "The environment we are provisioning, i.e. test, do not mistake this with the AWS account"
  type        = string
}

variable "is_production" {
  description = "Indicates whether the target account is configured with production-level settings"
  type        = bool
}

variable "postgres_instance_type" {
  description = "RDS instance type for individual environments"
  type        = string
}

variable "private_subnet_ids" {
  description = "List of private subnet IDs"
  type        = list(string)
}

variable "product" {
  description = "product's common attributes"
  type = object({
    name               = string
    resource_name      = string
    public_hosted_zone = string
  })
}

variable "public_hosted_zone_id" {
  description = "ID of the public hosted zone"
  type        = string
}

variable "public_subnet_ids" { # @TODO (ABN) burn me once migration is done
  description = "List of public subnet IDs while migrating using dumpfile"
  type        = list(string)
  default     = []
}

variable "role_db_import_arn" {
  description = "The ARN for the IAM role to be used as instance profile for dp-import boxes (EC2)."
  type        = string
}

variable "role_db_import_name" {
  description = "Name the IAM role to be used as instance profile for dp-import boxes (EC2)."
  type        = string
}

variable "role_rds_backup_arn" {
  description = "The ARN for the IAM role that permits users to access to S3 buckets holding dump files."
  type        = string
}

variable "role_rds_cloudwatch_arn" {
  description = "The ARN for the IAM role that permits RDS to send data to CloudWatch. Required in production accounts where enhanced monitoring is enabled"
  type        = string
  default     = ""
}

variable "role_terraform_arn" {
  description = "Terraform IAM role ARN"
  type        = string
}

variable "tags" {
  description = "Tags to apply to all resources in this module"
  type        = map(string)
}
