#!/bin/bash
set -e

CB_HOME="/opt/cloudbeaver"
WORKSPACE="${CB_HOME}/workspace"
RUNTIME_CONF="${WORKSPACE}/.data/.cloudbeaver.runtime.conf"
DATA_SOURCES_FILE="${WORKSPACE}/GlobalConfiguration/.dbeaver/data-sources.json"

echo "-----------------------------------"
echo "CloudBeaver custom entrypoint start"
echo "-----------------------------------"

# One-time cleanup for this rollout
if [ -d "$WORKSPACE" ]; then
  echo "🗑️  Removing .data and GlobalConfiguration for fresh initialization..."
  rm -rf "$WORKSPACE/.data"/*
  rm -rf "$WORKSPACE/GlobalConfiguration"
  echo "✅ Cleanup done, metadata untouched."
fi

echo "🔧 Writing server config and admin credentials (JSON) to $RUNTIME_CONF"
mkdir -p "$(dirname "$RUNTIME_CONF")"
cat > "$RUNTIME_CONF" <<EOF
{
  "server": {
    "name": "${CB_SERVER_NAME}",
    "url": "${CB_SERVER_URL}"
  },
  "admin": {
    "name": "${CB_ADMIN_NAME}",
    "password": "${CB_ADMIN_PASSWORD}"
  }
}
EOF
echo "✅ cloudbeaver.runtime.conf (JSON) created!"

echo "🔍 Printing cloudbeaver.runtime.conf content (cat -A):"
cat -A "$RUNTIME_CONF"

echo "🧩 Preparing CloudBeaver server configuration..."
mkdir -p "$(dirname "$DATA_SOURCES_FILE")"

if [ -n "$CLOUD_BEAVER_DATA_SOURCES" ]; then
  echo "✅ Writing data-sources.json from secret..."
  echo "$CLOUD_BEAVER_DATA_SOURCES" > "$DATA_SOURCES_FILE"
else
  echo "⚠️ CLOUD_BEAVER_DATA_SOURCES not set, skipping DB connection config."
fi

echo "Version 0.0.11"
echo "⚠️ Debugging: "
echo "ADMIN: $CB_ADMIN_NAME"
echo "URL: $CB_SERVER_URL"

echo "🔍 UID: $(id -u), GID: $(id -g)"
echo "📁 Verifying mount at /opt/cloudbeaver/workspace"
df -h /opt/cloudbeaver/workspace || echo "⚠️ Failed to inspect mount path"

echo "📂 Listing workspace contents:"
ls -l /opt/cloudbeaver/workspace || echo "⚠️ Failed to list /opt/cloudbeaver/workspace"

echo "🚀 Starting CloudBeaver with conf file..."
exec /opt/cloudbeaver/run-server.sh
