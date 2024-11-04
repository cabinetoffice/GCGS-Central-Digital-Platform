locals {
  tags = {
    component_root = "externals"
  }
}

inputs = {
  tags = merge(
    {
      external_team = "cdp-fts"
      life_time     = "during migration only"
    },
    local.tags
  )
}
