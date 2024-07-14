resource "aws_ecr_repository" "this" {
  for_each = toset(local.repositories)

  name                 = "cdp-${each.value}"
  image_tag_mutability = each.value != "grafana" ? "IMMUTABLE" : "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = var.tags
}
