moved {
  from = aws_sfn_state_machine.ecs_run_migration_cfs["cfs-migrations"]
  to   = aws_sfn_state_machine.ecs_run_migration_cfs
}

moved {
  from = aws_sfn_state_machine.ecs_run_migration_fts["fts-migrations"]
  to   = aws_sfn_state_machine.ecs_run_migration_fts
}

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
