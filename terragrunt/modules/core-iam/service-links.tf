resource "aws_iam_service_linked_role" "ecs" {
  aws_service_name = "ecs.amazonaws.com"
  description      = "Allows Amazon ECS to manage AWS resources on ${aws_iam_role.terraform.name} behalf"
  tags             = var.tags
}

resource "aws_iam_service_linked_role" "elasticloadbalancing" {
  aws_service_name = "elasticloadbalancing.amazonaws.com"
  description      = "Allows Amazon ELB to manage AWS resources on ${aws_iam_role.terraform.name} behalf"
  tags             = var.tags
}

resource "aws_iam_service_linked_role" "rds" {
  aws_service_name = "rds.amazonaws.com"
  description      = "Allows Amazon RDS to manage AWS resources on ${aws_iam_role.terraform.name} behalf"
  tags             = var.tags
}
