resource "aws_cloudwatch_event_rule" "ecr_push" {
  for_each = aws_ecr_repository.this

  name        = "${local.name_prefix}-ecr-push-to-${each.value.name}"
  description = "CloudWatch Event rule to detect ECR push events to ${each.value.name}"

  event_pattern = jsonencode(
    {
      "source" : ["aws.ecr"],
      "detail-type" : ["ECR Image Action"],
      "detail" : {
        "action-type" : ["PUSH"],
        "image-tag" : ["latest"],
        "repository-name" : [each.value.name]
        "result" : ["SUCCESS"],
      }
    }
  )

  tags = var.tags
}

# resource "aws_cloudwatch_event_target" "trigger_service_deployment" {
#   for_each = aws_cloudwatch_event_rule.ecr_push
#   rule     = each.value.name
#   arn      = each.key == "organisation-information-migrations" ? aws_sfn_state_machine.ecs_run_migration.arn : aws_sfn_state_machine.ecs_force_deploy[each.key].arn
#   role_arn = var.role_cloudwatch_events_arn
# }
