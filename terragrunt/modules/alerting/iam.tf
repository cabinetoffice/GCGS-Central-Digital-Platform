resource "aws_iam_role" "awsbot" {
  name = "${local.name_prefix}-awsbot"

  assume_role_policy = data.aws_iam_policy_document.assume_awsbot.json
}

resource "aws_iam_policy" "chatbot_notificationsonly" {
  name        = "${local.name_prefix}-chatbot-notificationsonly"
  description = "AWS Chatbot Notifications Only Policy"
  policy      = data.aws_iam_policy_document.awsbot.json
  tags        = var.tags
}

resource "aws_iam_role_policy_attachment" "amazon_q_full_access" {
  role       = aws_iam_role.awsbot.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonQFullAccess"
}

resource "aws_iam_role_policy_attachment" "chatbot_notificationsonly" {
  policy_arn = aws_iam_policy.chatbot_notificationsonly.arn
  role       = aws_iam_role.awsbot.name
}