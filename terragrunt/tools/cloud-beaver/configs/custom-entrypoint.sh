#!/bin/bash
set -e

echo "🔍 UID: $(id -u), GID: $(id -g)"
echo "📁 Verifying mount at /opt/cloudbeaver/workspace"
df -h /opt/cloudbeaver/workspace || echo "⚠️ Failed to inspect mount path"

echo "📂 Listing workspace contents:"
ls -l /opt/cloudbeaver/workspace || echo "⚠️ Failed to list /opt/cloudbeaver/workspace"

echo "📁 Assets available at:"
find /opt/cloudbeaver/web -type f | grep -E "\.js|\.css"

TARGET="/opt/cloudbeaver/workspace/.data/cb.h2v2.dat.mv.db"

if [ -f "$TARGET" ]; then
    echo "🧹 Removing locked H2 database file: $TARGET"
    rm -f "$TARGET"
else
    echo "✅ No lock file found at $TARGET"
fi

echo "🚀 Starting CloudBeaver..."
exec /opt/cloudbeaver/run-server.sh
