#!/bin/bash
set -euo pipefail

GRAFANA_HOME="${GRAFANA_HOME:-/usr/share/grafana}"
GRAFANA_CONFIG="${GRAFANA_CONFIG:-/etc/grafana/grafana.ini}"

sync_admin_password_from_env() {
  if [[ -z "${GF_SECURITY_ADMIN_PASSWORD:-}" ]]; then
    return 0
  fi

  local -a cli
  if command -v grafana >/dev/null 2>&1; then
    cli=("$(command -v grafana)" cli --homepath "${GRAFANA_HOME}" --config "${GRAFANA_CONFIG}")
  elif command -v grafana-cli >/dev/null 2>&1; then
    cli=("$(command -v grafana-cli)" --homepath "${GRAFANA_HOME}" --config "${GRAFANA_CONFIG}")
  elif [[ -x "${GRAFANA_HOME}/bin/grafana" ]]; then
    cli=("${GRAFANA_HOME}/bin/grafana" cli --homepath "${GRAFANA_HOME}" --config "${GRAFANA_CONFIG}")
  elif [[ -x "${GRAFANA_HOME}/bin/grafana-cli" ]]; then
    cli=("${GRAFANA_HOME}/bin/grafana-cli" --homepath "${GRAFANA_HOME}" --config "${GRAFANA_CONFIG}")
  else
    echo "WARN: grafana CLI not found; cannot sync admin password from Secrets Manager env var." >&2
    return 0
  fi

  echo "Syncing Grafana admin password from ECS secret (GF_SECURITY_ADMIN_PASSWORD)..." >&2
  echo "Using CLI: ${cli[*]}" >&2

  local attempts="${GRAFANA_ADMIN_PASSWORD_SYNC_ATTEMPTS:-20}"
  local sleep_seconds="${GRAFANA_ADMIN_PASSWORD_SYNC_SLEEP_SECONDS:-5}"
  local attempt=1

  while true; do
    if "${cli[@]}" admin reset-admin-password "${GF_SECURITY_ADMIN_PASSWORD}" >/tmp/grafana-admin-pass-sync.log 2>&1; then
      echo "Grafana admin password synced." >&2
      break
    fi

    if [[ "${attempt}" -ge "${attempts}" ]]; then
      echo "WARN: failed to sync Grafana admin password after ${attempts} attempts; continuing startup." >&2
      cat /tmp/grafana-admin-pass-sync.log >&2 || true
      break
    fi

    echo "Password sync attempt ${attempt}/${attempts} failed; retrying in ${sleep_seconds}s..." >&2
    sleep "${sleep_seconds}"
    attempt=$((attempt + 1))
  done
}

sync_admin_password_from_env
exec /run.sh
