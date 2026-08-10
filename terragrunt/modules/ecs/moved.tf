# Step Functions state machines were consolidated into a single resource (`ecs_run_task`).
# These moved blocks preserve existing SFN resources in-place and avoid recreation.

# Sirsi db-migrations (for_each)
moved {
  from = aws_sfn_state_machine.ecs_run_migration["organisation-information-migrations"]
  to   = aws_sfn_state_machine.ecs_run_task["organisation-information-migrations"]
}

moved {
  from = aws_sfn_state_machine.ecs_run_migration["entity-verification-migrations"]
  to   = aws_sfn_state_machine.ecs_run_task["entity-verification-migrations"]
}

# CFS/FTS/Pg migrations (historically had multiple addresses)
moved {
  from = aws_sfn_state_machine.ecs_run_migration_cfs["cfs-migrations"]
  to   = aws_sfn_state_machine.ecs_run_task["cfs-migrations"]
}

# If older state used a single instance address, first move it into the keyed instance.
moved {
  from = aws_sfn_state_machine.ecs_run_migration_cfs
  to   = aws_sfn_state_machine.ecs_run_migration_cfs["cfs-migrations"]
}

moved {
  from = aws_sfn_state_machine.ecs_run_migration_fts["fts-migrations"]
  to   = aws_sfn_state_machine.ecs_run_task["fts-migrations"]
}

# If older state used a single instance address, first move it into the keyed instance.
moved {
  from = aws_sfn_state_machine.ecs_run_migration_fts
  to   = aws_sfn_state_machine.ecs_run_migration_fts["fts-migrations"]
}

moved {
  from = aws_sfn_state_machine.ecs_run_migration_fts_postgres
  to   = aws_sfn_state_machine.ecs_run_task["fts-findtender-migrations"]
}

# One-off tasks (seeder)
moved {
  from = aws_sfn_state_machine.ecs_run_one_off_task["fts-ocds-export-seeder"]
  to   = aws_sfn_state_machine.ecs_run_task["fts-ocds-export-seeder"]
}

# Existing migration task definition moves (historical refactor)
moved {
  from = module.ecs_migration_tasks_cfs["cfs-migrations"].aws_ecs_task_definition.this
  to   = module.ecs_migration_task_cfs.aws_ecs_task_definition.this
}

moved {
  from = module.ecs_migration_tasks_cfs["cfs-migrations"].time_sleep.listener_rule_propagation
  to   = module.ecs_migration_task_cfs.time_sleep.listener_rule_propagation
}

moved {
  from = module.ecs_migration_tasks_fts["fts-migrations"].aws_ecs_task_definition.this
  to   = module.ecs_migration_task_fts.aws_ecs_task_definition.this
}

moved {
  from = module.ecs_migration_tasks_fts["fts-migrations"].time_sleep.listener_rule_propagation
  to   = module.ecs_migration_task_fts.time_sleep.listener_rule_propagation
}

moved {
  from = aws_sfn_state_machine.ecs_run_migration["user-management-migrations"]
  to   = aws_sfn_state_machine.ecs_run_task["user-management-migrations"]
}
