resource "aws_sfn_state_machine" "update_connection_string" {
  name     = "${var.db_name}-update-connection-string"
  role_arn = var.role_db_connection_step_function_arn
  tags     = var.tags

  definition = templatefile("${path.module}/templates/state-machine/update-connection-string.json.tftpl", {
    secret_connection_string_arn = aws_secretsmanager_secret.db_connection_string.arn,
    secret_database_creds_arn    = data.aws_secretsmanager_secret.postgres.arn
    db_server                    = local.db_address,
    db_database                  = local.db_name,
    db_username                  = local.db_username
  })
}
