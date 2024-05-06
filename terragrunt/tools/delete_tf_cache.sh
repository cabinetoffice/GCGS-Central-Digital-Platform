#!/usr/bin/env bash

root_dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )/.." && pwd )"

echo "🔎 Clearing Terraform and Terragrunt cache files..."
find "${root_dir}" -type d -name '.terragrunt-cache' -prune -exec echo "␡ Removing {}" \; -exec rm -rf '{}' +
find "${root_dir}" -type d -name '.terraform' -prune -exec echo "␡ Removing {}" \; -exec rm -rf '{}' +
find "${root_dir}" -type d -name '.terraform.d' -prune -exec echo "␡ Removing {}" \; -exec rm -rf '{}' +
find "${root_dir}" -type f -name '.terraform.lock.hcl' -delete
find "${root_dir}" -type f -name 'temp_providers.tf' -delete
echo "😎 Done."
