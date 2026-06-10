#!/usr/bin/env bash
set -euo pipefail

if ! command -v jq >/dev/null 2>&1; then
  echo "jq is required." >&2
  exit 1
fi

usage() {
  cat <<'EOF'
Usage:
  teams-graph.sh device-code
  teams-graph.sh token-from-device <device_code>
  teams-graph.sh refresh-token <refresh_token>
  teams-graph.sh post-message <team_id> <channel_id> <html_file>
  teams-graph.sh patch-message <team_id> <channel_id> <message_id> <html_file>

Required env vars:
  TENANT_ID
  CLIENT_ID
  CLIENT_SECRET (only for refresh-token)
  ACCESS_TOKEN (only for post/patch)

Notes:
  - device-code uses delegated permissions and returns user_code + device_code.
  - token-from-device returns access_token + refresh_token after you sign in.
  - refresh-token returns a new access_token (and possibly a new refresh_token).
  - post/patch read HTML content from a file and send it as message body.
EOF
}

require_env() {
  local name="$1"
  if [[ -z "${!name:-}" ]]; then
    echo "Missing env var: ${name}" >&2
    exit 1
  fi
}

cmd="${1:-}"
case "$cmd" in
  device-code)
    require_env TENANT_ID
    require_env CLIENT_ID
    curl -s -X POST "https://login.microsoftonline.com/${TENANT_ID}/oauth2/v2.0/devicecode" \
      -H "Content-Type: application/x-www-form-urlencoded" \
      -d "client_id=${CLIENT_ID}" \
      -d "scope=offline_access%20User.Read%20Chat.ReadWrite%20ChannelMessage.Send" | jq .
    ;;
  token-from-device)
    require_env TENANT_ID
    require_env CLIENT_ID
    device_code="${2:-}"
    if [[ -z "$device_code" ]]; then
      echo "device_code is required." >&2
      exit 1
    fi
    curl -s -X POST "https://login.microsoftonline.com/${TENANT_ID}/oauth2/v2.0/token" \
      -H "Content-Type: application/x-www-form-urlencoded" \
      -d "grant_type=urn:ietf:params:oauth:grant-type:device_code" \
      -d "client_id=${CLIENT_ID}" \
      -d "device_code=${device_code}" | jq .
    ;;
  refresh-token)
    require_env TENANT_ID
    require_env CLIENT_ID
    require_env CLIENT_SECRET
    refresh_token="${2:-}"
    if [[ -z "$refresh_token" ]]; then
      echo "refresh_token is required." >&2
      exit 1
    fi
    curl -s -X POST "https://login.microsoftonline.com/${TENANT_ID}/oauth2/v2.0/token" \
      -H "Content-Type: application/x-www-form-urlencoded" \
      -d "client_id=${CLIENT_ID}" \
      -d "client_secret=${CLIENT_SECRET}" \
      -d "grant_type=refresh_token" \
      -d "refresh_token=${refresh_token}" \
      -d "scope=offline_access%20User.Read%20Chat.ReadWrite%20ChannelMessage.Send" | jq .
    ;;
  post-message)
    require_env ACCESS_TOKEN
    team_id="${2:-}"
    channel_id="${3:-}"
    html_file="${4:-}"
    if [[ -z "$team_id" || -z "$channel_id" || -z "$html_file" ]]; then
      echo "team_id, channel_id, and html_file are required." >&2
      exit 1
    fi
    html_content=$(cat "$html_file" | jq -Rs .)
    curl -s -X POST \
      "https://graph.microsoft.com/v1.0/teams/${team_id}/channels/${channel_id}/messages" \
      -H "Authorization: Bearer ${ACCESS_TOKEN}" \
      -H "Content-Type: application/json" \
      -d "{\"body\":{\"contentType\":\"html\",\"content\":${html_content}}}" | jq .
    ;;
  patch-message)
    require_env ACCESS_TOKEN
    team_id="${2:-}"
    channel_id="${3:-}"
    message_id="${4:-}"
    html_file="${5:-}"
    if [[ -z "$team_id" || -z "$channel_id" || -z "$message_id" || -z "$html_file" ]]; then
      echo "team_id, channel_id, message_id, and html_file are required." >&2
      exit 1
    fi
    html_content=$(cat "$html_file" | jq -Rs .)
    curl -s -X PATCH \
      "https://graph.microsoft.com/v1.0/teams/${team_id}/channels/${channel_id}/messages/${message_id}" \
      -H "Authorization: Bearer ${ACCESS_TOKEN}" \
      -H "Content-Type: application/json" \
      -d "{\"body\":{\"contentType\":\"html\",\"content\":${html_content}}}" | jq .
    ;;
  *)
    usage
    exit 1
    ;;
esac
