data "aws_iam_policy_document" "scheduler_assume" {
  statement {
    effect  = "Allow"
    actions = ["sts:AssumeRole"]

    principals {
      type        = "Service"
      identifiers = ["scheduler.amazonaws.com"]
    }
  }
}

data "aws_iam_policy_document" "scheduler_start_pipeline" {
  statement {
    effect    = "Allow"
    actions   = ["codepipeline:StartPipelineExecution"]
    resources = [aws_codepipeline.weekly_refresh.arn]
  }
}

resource "aws_iam_role" "scheduler" {
  name               = "${local.name_prefix}-fts-db-weekly-refresh-scheduler"
  assume_role_policy = data.aws_iam_policy_document.scheduler_assume.json
  tags               = var.tags
}

resource "aws_iam_policy" "scheduler_start_pipeline" {
  name   = "${local.name_prefix}-fts-db-weekly-refresh-scheduler-start-pipeline"
  policy = data.aws_iam_policy_document.scheduler_start_pipeline.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "scheduler_start_pipeline" {
  role       = aws_iam_role.scheduler.name
  policy_arn = aws_iam_policy.scheduler_start_pipeline.arn
}
