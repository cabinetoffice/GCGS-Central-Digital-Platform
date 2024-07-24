resource "aws_iam_policy" "orchestrator_pipeline" {
  name   = "${local.name_prefix}-orchestrator-pipeline"
  policy = data.aws_iam_policy_document.orchestrator_pipeline.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "orchestrator_pipeline" {
  policy_arn = aws_iam_policy.orchestrator_pipeline.arn
  role       = var.ci_pipeline_role_name
}

resource "aws_iam_policy" "orchestrator_codebuild" {
  name   = "${local.name_prefix}-orchestrator-codebuild"
  policy = data.aws_iam_policy_document.orchestrator_codebuild.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "orchestrator_codebuild" {
  policy_arn = aws_iam_policy.orchestrator_codebuild.arn
  role       = var.ci_build_role_name
}

resource "aws_iam_policy" "cloudwatch_events_policy" {
  name   = "${local.name_prefix}-cloudwatch-event-trigger-pipeline"
  policy = data.aws_iam_policy_document.orchestrator_pipeline.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "cloudwatch_events_policy" {
  policy_arn = aws_iam_policy.orchestrator_pipeline.arn
  role       = var.role_cloudwatch_events_name
}
