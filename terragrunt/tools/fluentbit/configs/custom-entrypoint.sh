#!/bin/sh
set -e

CONFIG_DIR="/fluent-bit/etc"
DEFAULT_INPUT="$CONFIG_DIR/inputs/default-log-tail.conf"
DEFAULT_PARSER="$CONFIG_DIR/parsers/default-parser.conf"

mkdir -p "$CONFIG_DIR"
mkdir -p "$CONFIG_DIR/inputs"
mkdir -p "$CONFIG_DIR/parsers"

echo "[INFO] Checking for existing config in $CONFIG_DIR/custom.conf"
if [ "$FLUENTBIT_FORCE_INIT" = "true" ] || [ ! -f "$CONFIG_DIR/custom.conf" ]; then
cat <<EOF > "$CONFIG_DIR/custom.conf"
[SERVICE]
    Parsers_File parsers/*.conf

@INCLUDE inputs/*.conf

[INPUT]
    Name tcp
    Listen 0.0.0.0
    Port 2020
    Format none
    Tag health.ping

[OUTPUT]
    Name prometheus_exporter
    Match *
    Host 0.0.0.0
    Port 2021
EOF
fi

if [ ! -f "$DEFAULT_INPUT" ]; then
  echo "[INFO] Writing default input config to $DEFAULT_INPUT"
  cat <<EOF > "$DEFAULT_INPUT"
[INPUT]
    Name              tail
    Path              /var/log/supervisor/*.log
    Path_Key          filename
    Tag               default.supervisor
    DB                /var/log/flb-default.db
    Mem_Buf_Limit     5MB
    Skip_Long_Lines   On
EOF
fi

if [ ! -f "$DEFAULT_PARSER" ]; then
  echo "[INFO] Writing default parser config to $DEFAULT_PARSER"
  cat <<EOF > "$DEFAULT_PARSER"
[PARSER]
    Name   filename_parser
    Format regex
    Regex  ^.*/(?<process>[^.]+)\\.(?<stream>[^.]+)\\.log$
EOF
fi

echo "[INFO] Contents of custom.conf"
cat /fluent-bit/etc/custom.conf

echo "[INFO] Checking for dynamic input configs..."
if [ -d "/fluent-bit/etc/inputs" ]; then
  echo "[INFO] Contents of /fluent-bit/etc/inputs/:"
  ls -lh /fluent-bit/etc/inputs/
else
  echo "[WARN] No /fluent-bit/etc/inputs/ directory found."
fi
if [ -d "/fluent-bit/etc/parsers" ]; then
  echo "[INFO] Contents of /fluent-bit/etc/parsers/:"
  ls -lh /fluent-bit/etc/parsers/
else
  echo "[WARN] No /fluent-bit/etc/parsers/ directory found."
fi


/fluent-bit/bin/fluent-bit -c "$CONFIG_DIR/custom.conf" --dry-run --verbose || {
  echo "[ERROR] Config is invalid"
  exit 1
}

exec /fluent-bit/bin/fluent-bit -c "$CONFIG_DIR/custom.conf"
