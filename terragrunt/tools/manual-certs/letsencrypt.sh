#!/bin/bash
set -e

# === Usage Check ===
if [[ -z "$1" ]]; then
  echo "Usage: $0 <path_to_cert_folder>"
  exit 1
fi

CERT_DIR="$1"
REGION="eu-west-2"

FULLCHAIN="$CERT_DIR/fullchain.pem"
PRIVATE_KEY="$CERT_DIR/privkey.pem"
CERT="$CERT_DIR/cert.pem"
CHAIN="$CERT_DIR/chain.pem"

# === Check files exist ===
if [[ ! -f "$FULLCHAIN" || ! -f "$PRIVATE_KEY" ]]; then
  echo "Error: $FULLCHAIN or $PRIVATE_KEY not found."
  exit 1
fi

# === Split fullchain.pem ===
echo "Splitting $FULLCHAIN into $CERT and $CHAIN ..."
awk 'BEGIN {c=0} /BEGIN CERTIFICATE/{c++} {print > (dir "/part" c ".pem")}' dir="$CERT_DIR/" "$FULLCHAIN"

mv "$CERT_DIR/part1.pem" "$CERT"
if [[ -f "$CERT_DIR/part2.pem" ]]; then
  mv "$CERT_DIR/part2.pem" "$CHAIN"
  echo "Split successful: cert.pem and chain.pem created."
else
  echo "No intermediate cert found — creating empty chain.pem."
  touch "$CHAIN"
fi

# === Import to ACM ===
echo "Importing certificate to ACM ..."
AWS_CMD_STR="${AWS_CMD:-aws}"
read -r -a AWS_CMD_ARR <<<"$AWS_CMD_STR"

CERT_ARN=$("${AWS_CMD_ARR[@]}" acm import-certificate \
  --certificate fileb://"$CERT" \
  --certificate-chain fileb://"$CHAIN" \
  --private-key fileb://"$PRIVATE_KEY" \
  --region "$REGION" \
  --query CertificateArn \
  --output text)

echo "✅ Certificate imported successfully!"
echo "🔐 Certificate ARN: $CERT_ARN"

if [[ -n "${CERT_ARN_FILE:-}" ]]; then
  mkdir -p "$(dirname "$CERT_ARN_FILE")"
  echo "$CERT_ARN" > "$CERT_ARN_FILE"
fi
