#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Restores archived ECR images listed in an inventory JSONL (typically produced by ecs_image_inventory.sh).

Default behavior:
  - Restores only images that belong to the CURRENT caller account (so you can run this
    while assumed into the orchestrator account and it will only touch orchestrator ECR).
  - Skips missing images/tags silently.

Requirements:
  - aws cli v2, jq
  - IAM perms: sts:GetCallerIdentity, ecr:DescribeImages, ecr:UpdateImageStorageClass

Usage:
  ecr_restore_archived_images_from_inventory.sh --inventory <file.jsonl> [--profile <profile>] [--apply] [--max-restore <n>] [--only-account <12-digit>]
EOF
}

inventory=""
profile=""
apply="0"
max_restore="0"
only_account=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --inventory)
      inventory="${2:-}"; shift 2
      ;;
    --profile)
      profile="${2:-}"; shift 2
      ;;
    --apply)
      apply="1"; shift
      ;;
    --max-restore)
      max_restore="${2:-0}"; shift 2
      ;;
    --only-account)
      only_account="${2:-}"; shift 2
      ;;
    -h|--help)
      usage; exit 0
      ;;
    *)
      echo "Unknown arg: $1" >&2
      usage >&2
      exit 2
      ;;
  esac
done

if [[ -z "${inventory}" ]]; then
  echo "Missing --inventory <file.jsonl>." >&2
  exit 2
fi
if [[ ! -f "${inventory}" ]]; then
  echo "Inventory not found: ${inventory}" >&2
  exit 2
fi

aws_cli() {
  if [[ -n "${profile}" ]]; then
    aws --profile "${profile}" "$@"
  else
    aws "$@"
  fi
}

tmpdir="$(mktemp -d)"
trap 'rm -rf "${tmpdir}"' EXIT

caller_account="$(aws_cli sts get-caller-identity --query Account --output text)"
if [[ -z "${only_account}" ]]; then
  only_account="${caller_account}"
fi

echo "Caller account: ${caller_account}" >&2
if [[ -n "${profile}" ]]; then echo "Profile: ${profile}" >&2; fi
echo "Inventory: ${inventory}" >&2
echo "Restoring only account: ${only_account}" >&2
if [[ "${apply}" == "1" ]]; then echo "Mode: APPLY" >&2; else echo "Mode: DRY RUN" >&2; fi

restore_count="0"
skipped_other_account="0"
missing_count="0"
checked_count="0"

describe_one() {
  local region="$1"
  local repo="$2"
  local id_kind="$3" # tag|digest
  local id_value="$4"

  local err_file out status digest
  err_file="${tmpdir}/ecr.describe.err.$RANDOM"

  if [[ "${id_kind}" == "tag" ]]; then
    if out="$(aws_cli ecr describe-images \
      --region "${region}" \
      --repository-name "${repo}" \
      --image-ids imageTag="${id_value}" \
      --query 'imageDetails[0].[imageStatus,imageDigest]' \
      --output text 2>"${err_file}")"; then
      status="$(awk '{print $1}' <<<"${out}")"
      digest="$(awk '{print $2}' <<<"${out}")"
      echo "${status} ${digest}"
      return 0
    fi
  else
    if out="$(aws_cli ecr describe-images \
      --region "${region}" \
      --repository-name "${repo}" \
      --image-ids imageDigest="${id_value}" \
      --query 'imageDetails[0].[imageStatus,imageDigest]' \
      --output text 2>"${err_file}")"; then
      status="$(awk '{print $1}' <<<"${out}")"
      digest="$(awk '{print $2}' <<<"${out}")"
      echo "${status} ${digest}"
      return 0
    fi
  fi

  if grep -q 'ImageNotFoundException' "${err_file}"; then
    echo "MISSING"
    return 0
  fi

  # Other errors: surface them but keep going.
  echo "ERROR $(tr '\n' ' ' <"${err_file}")"
  return 0
}

restore_digest() {
  local region="$1"
  local repo="$2"
  local digest="$3"

  if [[ "${apply}" == "1" ]]; then
    aws_cli ecr update-image-storage-class \
      --region "${region}" \
      --repository-name "${repo}" \
      --image-id imageDigest="${digest}" \
      --target-storage-class STANDARD \
      --output json >/dev/null
  else
    echo "DRY RUN: aws ecr update-image-storage-class --region ${region} --repository-name ${repo} --image-id imageDigest=${digest} --target-storage-class STANDARD"
  fi
}

# Reduce to unique ECR identifiers per (account, region, repo, tag, digest).
targets_json="${tmpdir}/targets.json"
jq -s '
  [
    .[]
    | select(.ecr == true)
    | {
        account: .account,
        region: .imageRegion,
        repo: .repo,
        tag: (.tag // null),
        digest: (.digest // null)
      }
  ]
  | unique_by([.account,.region,.repo,.tag,.digest])
' "${inventory}" > "${targets_json}"

target_count="$(jq 'length' "${targets_json}")"
echo "Unique ECR image references in inventory: ${target_count}" >&2
if [[ "${target_count}" -eq 0 ]]; then
  exit 0
fi

while IFS= read -r t; do
  account="$(jq -r '.account' <<<"${t}")"
  region="$(jq -r '.region' <<<"${t}")"
  repo="$(jq -r '.repo' <<<"${t}")"
  tag="$(jq -r '.tag // empty' <<<"${t}")"
  digest="$(jq -r '.digest // empty' <<<"${t}")"

  if [[ "${account}" != "${only_account}" ]]; then
    skipped_other_account="$((skipped_other_account + 1))"
    continue
  fi

  checked_count="$((checked_count + 1))"

  if [[ -n "${digest}" ]]; then
    res="$(describe_one "${region}" "${repo}" "digest" "${digest}")"
  else
    res="$(describe_one "${region}" "${repo}" "tag" "${tag}")"
  fi

  if [[ "${res}" == "MISSING" ]]; then
    missing_count="$((missing_count + 1))"
    continue
  fi
  if [[ "${res}" == ERROR* ]]; then
    echo "WARN: ${account}/${region}/${repo} ${tag}${digest:+@${digest}}: ${res#ERROR }" >&2
    continue
  fi

  status="$(awk '{print $1}' <<<"${res}")"
  got_digest="$(awk '{print $2}' <<<"${res}")"

  if [[ "${status}" == "ARCHIVED" && -n "${got_digest}" && "${got_digest}" != "None" ]]; then
    if [[ "${max_restore}" -gt 0 && "${restore_count}" -ge "${max_restore}" ]]; then
      echo "Reached --max-restore=${max_restore}; stopping." >&2
      break
    fi
    restore_digest "${region}" "${repo}" "${got_digest}"
    restore_count="$((restore_count + 1))"
  fi
done < <(jq -c '.[]' "${targets_json}")

echo "Done. Checked=${checked_count} RestoresTriggered=${restore_count} Missing=${missing_count} SkippedOtherAccount=${skipped_other_account}" >&2
