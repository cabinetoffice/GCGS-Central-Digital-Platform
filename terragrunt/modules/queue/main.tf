module "entity_verification_queue" {
  source = "../sqs"

  message_retention_seconds = 1209600 # 14 days
  name                      = local.name_entity_verification_queue
  role_consumer_arn         = [var.role_ecs_task_arn]
  role_publisher_arn        = [var.role_ecs_task_arn]
  tags                      = var.tags
}

module "organisation_queue" {
  source = "../sqs"

  message_retention_seconds = 1209600 # 14 days
  name                      = local.name_organisation_queue
  role_consumer_arn         = [var.role_ecs_task_arn]
  role_publisher_arn        = [var.role_ecs_task_arn]
  tags                      = var.tags
}

module "av_scanner_queue" {
  source = "../sqs"

  message_retention_seconds = 1209600 # 14 days
  name                      = local.name_av_scanner_queue
  role_consumer_arn         = [var.role_ecs_task_arn]
  role_publisher_arn        = [var.role_ecs_task_arn]
  tags                      = var.tags
}
