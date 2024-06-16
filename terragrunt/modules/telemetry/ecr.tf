resource "aws_ecr_repository" "grafana" {
  name                 = "cdp-${var.grafana_config.name}"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = var.tags
}
