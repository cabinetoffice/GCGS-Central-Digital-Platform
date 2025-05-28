# resource "aws_dms_replication_instance" "this" {
#   replication_instance_id        = "dms-replication-instance"
#   replication_instance_class     = "dms.t3.medium"
#   allocated_storage              = 100
#   publicly_accessible            = true
#   multi_az                       = false
#   engine_version                 = "3.4.6"
#   auto_minor_version_upgrade     = true
#   apply_immediately              = true
# }
#
# resource "aws_dms_endpoint" "source" {
#   endpoint_id     = "source-mysql"
#   endpoint_type   = "source"
#   engine_name     = "mysql"
#   username        = var.source_username
#   password        = var.source_password
#   server_name     = var.source_server
#   port            = 3306
#   database_name   = var.source_db
#   ssl_mode        = "none"
# }
#
# resource "aws_dms_endpoint" "target" {
#   endpoint_id     = "target-mysql"
#   endpoint_type   = "target"
#   engine_name     = "mysql"
#   username        = var.target_username
#   password        = var.target_password
#   server_name     = var.target_server
#   port            = 3306
#   database_name   = var.target_db
#   ssl_mode        = "none"
# }
#
# resource "aws_dms_replication_task" "this" {
#   replication_task_id          = "mysql-migration-task"
#   source_endpoint_arn          = aws_dms_endpoint.source.endpoint_arn
#   target_endpoint_arn          = aws_dms_endpoint.target.endpoint_arn
#   replication_instance_arn     = aws_dms_replication_instance.this.replication_instance_arn
#   migration_type               = "full-load"
#
#   table_mappings = jsonencode({
#     rules = [
#       {
#         "rule-type" : "selection",
#         "rule-id" : "1",
#         "rule-name" : "1",
#         "object-locator" : {
#           "schema-name" : "%",
#           "table-name" : "%"
#         },
#         "rule-action" : "include"
#       }
#     ]
#   })
#
#   replication_task_settings = jsonencode({
#     TargetMetadata : {
#       TargetSchema : "",
#       SupportLobs : true,
#       FullLobMode : true,
#       LobChunkSize : 64,
#       LimitedSizeLobMode : false,
#       LoadMaxFileSize : 0,
#       ParallelLoadThreads : 0,
#       ParallelLoadBufferSize : 0,
#       BatchApplyEnabled : false,
#       TaskRecoveryTableEnabled : false
#     }
#   })
# }
