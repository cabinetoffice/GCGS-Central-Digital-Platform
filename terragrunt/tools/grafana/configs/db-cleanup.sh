#!/bin/bash
set -e

RULE_UID="ecs-cpu-high"

if ! command -v psql >/dev/null 2>&1; then
  echo "db-cleanup: psql not available, skipping" >&2
  exit 0
fi

if [ -n "${GF_DATABASE_URL:-}" ]; then
  CONN_STRING="${GF_DATABASE_URL}"
else
  if [ -z "${GF_DATABASE_HOST:-}" ] || [ -z "${GF_DATABASE_NAME:-}" ] || [ -z "${GF_DATABASE_USER:-}" ]; then
    echo "db-cleanup: missing GF_DATABASE_* env vars, skipping" >&2
    exit 0
  fi
  HOST="${GF_DATABASE_HOST%%:*}"
  PORT="${GF_DATABASE_HOST##*:}"
  if [ "$HOST" = "$PORT" ]; then
    PORT="5432"
  fi
  export PGHOST="${HOST}"
  export PGPORT="${PORT}"
  export PGDATABASE="${GF_DATABASE_NAME}"
  export PGUSER="${GF_DATABASE_USER}"
  export PGPASSWORD="${GF_DATABASE_PASSWORD:-}"
  export PGSSLMODE="${GF_DATABASE_SSL_MODE:-disable}"
  CONN_STRING=""
fi

SQL=$(cat <<'SQL_EOF'
DO $$
DECLARE
  rule_id bigint;
BEGIN
  SELECT id INTO rule_id FROM alert_rule WHERE uid = 'ecs-cpu-high';
  IF rule_id IS NULL THEN
    RAISE NOTICE 'db-cleanup: rule % not found', 'ecs-cpu-high';
    RETURN;
  END IF;

  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name='alert_rule_tag') THEN
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='alert_rule_tag' AND column_name='rule_id') THEN
      EXECUTE 'DELETE FROM alert_rule_tag WHERE rule_id = ' || rule_id;
    ELSIF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='alert_rule_tag' AND column_name='rule_uid') THEN
      EXECUTE 'DELETE FROM alert_rule_tag WHERE rule_uid = ''ecs-cpu-high''';
    END IF;
  END IF;

  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name='alert_rule_state') THEN
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='alert_rule_state' AND column_name='rule_id') THEN
      EXECUTE 'DELETE FROM alert_rule_state WHERE rule_id = ' || rule_id;
    ELSIF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='alert_rule_state' AND column_name='rule_uid') THEN
      EXECUTE 'DELETE FROM alert_rule_state WHERE rule_uid = ''ecs-cpu-high''';
    END IF;
  END IF;

  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name='alert_rule_version') THEN
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='alert_rule_version' AND column_name='rule_id') THEN
      EXECUTE 'DELETE FROM alert_rule_version WHERE rule_id = ' || rule_id;
    ELSIF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='alert_rule_version' AND column_name='rule_uid') THEN
      EXECUTE 'DELETE FROM alert_rule_version WHERE rule_uid = ''ecs-cpu-high''';
    END IF;
  END IF;

  DELETE FROM alert_rule WHERE id = rule_id;
END
$$;
SQL_EOF
)

if [ -n "${CONN_STRING}" ]; then
  psql "${CONN_STRING}" -v ON_ERROR_STOP=1 -c "${SQL}"
else
  psql -v ON_ERROR_STOP=1 -c "${SQL}"
fi
