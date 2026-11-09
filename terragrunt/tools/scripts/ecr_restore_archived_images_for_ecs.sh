#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Restores archived ECR images that are referenced by ECS services (current deployments).

Requirements:
  - aws cli v2, jq
  - IAM perms: ecs:ListClusters ecs:ListServices ecs:DescribeServices ecs:DescribeTaskDefinition
               ecr:DescribeImages ecr:UpdateImageStorageClass
  - If images live in a different AWS account: sts:AssumeRole in the caller account,
    plus a role in the target (ECR) account that allows the ECR actions.

Usage:
  ecr_restore_archived_images_for_ecs.sh --region <aws-region> [--profile <profile>] [--clusters-regex <regex>] [--apply] [--max-restore <n>] [--ecr-role-name <role>]

Notes:
  - Default is DRY RUN (prints what it would restore). Use --apply to perform restores.
  - Images in ECR Archive cannot be pulled until restored (can take ~minutes).
EOF
}

region="${AWS_REGION:-${AWS_DEFAULT_REGION:-}}"
profile=""
clusters_regex=""
apply="0"
max_restore="0"
ecr_role_name=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --region)
      region="${2:-}"; shift 2
      ;;
    --profile)
      profile="${2:-}"; shift 2
      ;;
    --clusters-regex)
      clusters_regex="${2:-}"; shift 2
      ;;
    --ecr-role-name)
      ecr_role_name="${2:-}"; shift 2
      ;;
    --apply)
      apply="1"; shift
      ;;
    --max-restore)
      max_restore="${2:-0}"; shift 2
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

if [[ -z "${region}" ]]; then
  echo "Missing --region (or set AWS_REGION/AWS_DEFAULT_REGION)." >&2
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

base_account="$(aws_cli sts get-caller-identity --query Account --output text)"
echo "Caller account: ${base_account}" >&2

echo "Region: ${region}" >&2
if [[ -n "${profile}" ]]; then echo "Profile: ${profile}" >&2; fi
if [[ -n "${clusters_regex}" ]]; then echo "Cluster filter regex: ${clusters_regex}" >&2; fi
if [[ -n "${ecr_role_name}" ]]; then echo "ECR cross-account role name: ${ecr_role_name}" >&2; fi
if [[ "${apply}" == "1" ]]; then echo "Mode: APPLY" >&2; else echo "Mode: DRY RUN" >&2; fi

assume_role_for_account() {
  local account="$1"
  local cache_file="${tmpdir}/assume-role.${account}.env"
  if [[ -f "${cache_file}" ]]; then
    # shellcheck disable=SC1090
    source "${cache_file}"
    return 0
  fi

  local role_arn="arn:aws:iam::${account}:role/${ecr_role_name}"
  local session_name="ecr-restore-$(date -u +%Y%m%dT%H%M%SZ)"
  local creds_json

  creds_json="$(aws_cli sts assume-role \
    --role-arn "${role_arn}" \
    --role-session-name "${session_name}" \
    --query 'Credentials' \
    --output json)"

  {
    echo "export AWS_ACCESS_KEY_ID=$(jq -r '.AccessKeyId' <<<"${creds_json}")"
    echo "export AWS_SECRET_ACCESS_KEY=$(jq -r '.SecretAccessKey' <<<"${creds_json}")"
    echo "export AWS_SESSION_TOKEN=$(jq -r '.SessionToken' <<<"${creds_json}")"
  } > "${cache_file}"

  # shellcheck disable=SC1090
  source "${cache_file}"
}

aws_ecr() {
  local target_account="$1"; shift

  if [[ -n "${ecr_role_name}" && "${target_account}" != "${base_account}" ]]; then
    (
      assume_role_for_account "${target_account}"
      aws "$@"
    )
  else
    aws_cli "$@"
  fi
}

clusters_json="${tmpdir}/clusters.json"
aws_cli ecs list-clusters --region "${region}" --output json > "${clusters_json}"

clusters_filtered_json="${tmpdir}/clusters.filtered.json"
if [[ -n "${clusters_regex}" ]]; then
  jq --arg re "${clusters_regex}" '
    .clusterArns
    | map(select((split("/")[-1]) | test($re)))
  ' "${clusters_json}" > "${clusters_filtered_json}"
else
  jq '.clusterArns' "${clusters_json}" > "${clusters_filtered_json}"
fi

cluster_count="$(jq 'length' "${clusters_filtered_json}")"
if [[ "${cluster_count}" -eq 0 ]]; then
  echo "No clusters found in ${region} (after filtering)." >&2
  exit 0
fi
echo "Clusters: ${cluster_count}" >&2

taskdefs_jsonl="${tmpdir}/taskdefs.jsonl"
: > "${taskdefs_jsonl}"

while IFS= read -r cluster_arn; do
  echo "Scanning cluster: ${cluster_arn}" >&2
  services_json="${tmpdir}/services.$(echo "${cluster_arn}" | tr '/:' '__').json"
  aws_cli ecs list-services --region "${region}" --cluster "${cluster_arn}" --output json > "${services_json}"

  mapfile -t service_arns < <(jq -r '.serviceArns[]?' "${services_json}")
  if [[ "${#service_arns[@]}" -eq 0 ]]; then
    continue
  fi

  # describe-services takes a limited number of ARNs per call; chunk to 10.
  for ((i=0; i<${#service_arns[@]}; i+=10)); do
    chunk=("${service_arns[@]:i:10}")
    desc_json="${tmpdir}/describe-services.$RANDOM.json"
    aws_cli ecs describe-services --region "${region}" --cluster "${cluster_arn}" --services "${chunk[@]}" --output json > "${desc_json}"
    jq -r '
      .services[]
      | ([.taskDefinition] + ((.deployments // []) | map(.taskDefinition)) | unique)
      | .[]?
    ' "${desc_json}" >> "${taskdefs_jsonl}"
  done
done < <(jq -r '.[]' "${clusters_filtered_json}")

sort -u "${taskdefs_jsonl}" > "${tmpdir}/taskdefs.unique.txt"
taskdef_count="$(wc -l < "${tmpdir}/taskdefs.unique.txt" | tr -d ' ')"
if [[ "${taskdef_count}" -eq 0 ]]; then
  echo "No ECS service task definitions found." >&2
  exit 0
fi
echo "Task definitions: ${taskdef_count}" >&2

images_jsonl="${tmpdir}/images.jsonl"
: > "${images_jsonl}"

while IFS= read -r td_arn; do
  td_json="${tmpdir}/taskdef.$RANDOM.json"
  aws_cli ecs describe-task-definition --region "${region}" --task-definition "${td_arn}" --output json > "${td_json}"
  jq -c --arg td "${td_arn}" '
    .taskDefinition.containerDefinitions[]
    | { taskDefinitionArn: $td, image: .image }
  ' "${td_json}" >> "${images_jsonl}"
done < "${tmpdir}/taskdefs.unique.txt"

targets_json="${tmpdir}/targets.json"
jq -s '
  [
    .[]
    | .image as $img
    | (
        try ($img | capture("^(?<account>[0-9]{12})\\.dkr\\.ecr\\.(?<region>[a-z0-9-]+)\\.amazonaws\\.com\\/(?<repo>[^:@]+)(?:(?::(?<tag>[^@]+))|(?:@(?<digest>sha256:[0-9a-f]{64})))$")) catch empty
      ) as $p
    | select($p != null)
    | {
        account: $p.account,
        region: $p.region,
        repo: $p.repo,
        tag: ($p.tag // null),
        digest: ($p.digest // null),
        taskDefinitionArn: .taskDefinitionArn,
        image: $img
      }
  ]
  | unique_by([.account,.region,.repo,.tag,.digest])
' "${images_jsonl}" > "${targets_json}"

target_count="$(jq 'length' "${targets_json}")"
if [[ "${target_count}" -eq 0 ]]; then
  echo "No private ECR images found in ECS task definitions." >&2
  exit 0
fi
echo "Unique ECR image references: ${target_count}" >&2

restore_count="0"

describe_tag_archived_digest() {
  local account="$1"
  local img_region="$2"
  local repo="$3"
  local tag="$4"

  local err_file out status digest
  err_file="${tmpdir}/ecr.describe.err.$RANDOM"
  if out="$(aws_ecr "${account}" ecr describe-images \
      --region "${img_region}" \
      --registry-id "${account}" \
      --repository-name "${repo}" \
      --image-ids imageTag="${tag}" \
      --query 'imageDetails[0].[imageStatus,imageDigest]' \
      --output text 2>"${err_file}")"; then
    status="$(awk '{print $1}' <<<"${out}")"
    digest="$(awk '{print $2}' <<<"${out}")"
    if [[ "${status}" == "ARCHIVED" && -n "${digest}" && "${digest}" != "None" ]]; then
      echo "${digest}"
    fi
    return 0
  fi

  if grep -q 'ImageNotFoundException' "${err_file}"; then
    return 0
  fi
  echo "WARN: describe-images failed for ${account}/${img_region}/${repo}:${tag}: $(tr '\n' ' ' <"${err_file}")" >&2
  return 0
}

describe_digest_archived_digest() {
  local account="$1"
  local img_region="$2"
  local repo="$3"
  local digest="$4"

  local err_file out status out_digest
  err_file="${tmpdir}/ecr.describe.err.$RANDOM"
  if out="$(aws_ecr "${account}" ecr describe-images \
      --region "${img_region}" \
      --registry-id "${account}" \
      --repository-name "${repo}" \
      --image-ids imageDigest="${digest}" \
      --query 'imageDetails[0].[imageStatus,imageDigest]' \
      --output text 2>"${err_file}")"; then
    status="$(awk '{print $1}' <<<"${out}")"
    out_digest="$(awk '{print $2}' <<<"${out}")"
    if [[ "${status}" == "ARCHIVED" && -n "${out_digest}" && "${out_digest}" != "None" ]]; then
      echo "${out_digest}"
    fi
    return 0
  fi

  if grep -q 'ImageNotFoundException' "${err_file}"; then
    return 0
  fi
  echo "WARN: describe-images failed for ${account}/${img_region}/${repo}@${digest}: $(tr '\n' ' ' <"${err_file}")" >&2
  return 0
}

restore_digest() {
  local account="$1"
  local img_region="$2"
  local repo="$3"
  local digest="$4"

  if [[ "${apply}" == "1" ]]; then
    aws_ecr "${account}" ecr update-image-storage-class \
      --region "${img_region}" \
      --registry-id "${account}" \
      --repository-name "${repo}" \
      --image-id imageDigest="${digest}" \
      --target-storage-class STANDARD \
      --output json >/dev/null
  else
    echo "DRY RUN: aws ecr update-image-storage-class --region ${img_region} --registry-id ${account} --repository-name ${repo} --image-id imageDigest=${digest} --target-storage-class STANDARD"
  fi
}

echo "Checking ECR image status and restoring ARCHIVED images..." >&2

# Group by (account, region, repo) and batch describe-images by tags/digests (100 per call).
while IFS= read -r group; do
  account="$(jq -r '.account' <<<"${group}")"
  img_region="$(jq -r '.region' <<<"${group}")"
  repo="$(jq -r '.repo' <<<"${group}")"

  mapfile -t tags < <(jq -r '.tags[]?' <<<"${group}")
  mapfile -t digests < <(jq -r '.digests[]?' <<<"${group}")

  if [[ "${#tags[@]}" -gt 0 ]]; then
    for ((i=0; i<${#tags[@]}; i+=100)); do
      batch=("${tags[@]:i:100}")
      ids_json="${tmpdir}/imageids.$RANDOM.json"
      jq -n --arg repo "${repo}" --arg reg "${account}" --argjson ids "$(printf '%s\n' "${batch[@]}" | jq -R . | jq -s 'map({imageTag: .})')" \
        '{registryId: $reg, repositoryName: $repo, imageIds: $ids}' > "${ids_json}"

      resp_json="${tmpdir}/describe-images.$RANDOM.json"
      err_file="${tmpdir}/describe-images.$RANDOM.err"
      if ! aws_ecr "${account}" ecr describe-images --region "${img_region}" --cli-input-json "file://${ids_json}" --output json > "${resp_json}" 2>"${err_file}"; then
        # If any tag in the batch is missing, ECR may fail the whole request. Fall back to per-tag queries.
        if ! grep -q 'ImageNotFoundException' "${err_file}"; then
          echo "WARN: describe-images failed for ${account}/${img_region}/${repo} (tags batch): $(tr '\n' ' ' <\"${err_file}\")" >&2
        fi
        for tag in "${batch[@]}"; do
          digest="$(describe_tag_archived_digest "${account}" "${img_region}" "${repo}" "${tag}")"
          if [[ -z "${digest}" ]]; then
            continue
          fi
          if [[ "${max_restore}" -gt 0 && "${restore_count}" -ge "${max_restore}" ]]; then
            echo "Reached --max-restore=${max_restore}; stopping." >&2
            exit 0
          fi
          restore_digest "${account}" "${img_region}" "${repo}" "${digest}"
          restore_count="$((restore_count + 1))"
        done
        continue
      fi

      jq -r '.failures[]? | "WARN: describe-images failure: " + (.failureCode // "") + " " + (.failureReason // "") + " " + (.imageId.imageTag // .imageId.imageDigest // "")' "${resp_json}" >&2 || true

      while IFS= read -r digest; do
        if [[ -z "${digest}" ]]; then continue; fi
        if [[ "${max_restore}" -gt 0 && "${restore_count}" -ge "${max_restore}" ]]; then
          echo "Reached --max-restore=${max_restore}; stopping." >&2
          exit 0
        fi
        restore_digest "${account}" "${img_region}" "${repo}" "${digest}"
        restore_count="$((restore_count + 1))"
      done < <(jq -r '.imageDetails[]? | select(.imageStatus=="ARCHIVED") | .imageDigest' "${resp_json}" | sort -u)
    done
  fi

  if [[ "${#digests[@]}" -gt 0 ]]; then
    for ((i=0; i<${#digests[@]}; i+=100)); do
      batch=("${digests[@]:i:100}")
      ids_json="${tmpdir}/imageids.$RANDOM.json"
      jq -n --arg repo "${repo}" --arg reg "${account}" --argjson ids "$(printf '%s\n' "${batch[@]}" | jq -R . | jq -s 'map({imageDigest: .})')" \
        '{registryId: $reg, repositoryName: $repo, imageIds: $ids}' > "${ids_json}"

      resp_json="${tmpdir}/describe-images.$RANDOM.json"
      err_file="${tmpdir}/describe-images.$RANDOM.err"
      if ! aws_ecr "${account}" ecr describe-images --region "${img_region}" --cli-input-json "file://${ids_json}" --output json > "${resp_json}" 2>"${err_file}"; then
        # If any digest in the batch is missing, ECR may fail the whole request. Fall back to per-digest queries.
        if ! grep -q 'ImageNotFoundException' "${err_file}"; then
          echo "WARN: describe-images failed for ${account}/${img_region}/${repo} (digests batch): $(tr '\n' ' ' <\"${err_file}\")" >&2
        fi
        for digest in "${batch[@]}"; do
          archived_digest="$(describe_digest_archived_digest "${account}" "${img_region}" "${repo}" "${digest}")"
          if [[ -z "${archived_digest}" ]]; then
            continue
          fi
          if [[ "${max_restore}" -gt 0 && "${restore_count}" -ge "${max_restore}" ]]; then
            echo "Reached --max-restore=${max_restore}; stopping." >&2
            exit 0
          fi
          restore_digest "${account}" "${img_region}" "${repo}" "${archived_digest}"
          restore_count="$((restore_count + 1))"
        done
        continue
      fi

      jq -r '.failures[]? | "WARN: describe-images failure: " + (.failureCode // "") + " " + (.failureReason // "") + " " + (.imageId.imageTag // .imageId.imageDigest // "")' "${resp_json}" >&2 || true

      while IFS= read -r digest; do
        if [[ -z "${digest}" ]]; then continue; fi
        if [[ "${max_restore}" -gt 0 && "${restore_count}" -ge "${max_restore}" ]]; then
          echo "Reached --max-restore=${max_restore}; stopping." >&2
          exit 0
        fi
        restore_digest "${account}" "${img_region}" "${repo}" "${digest}"
        restore_count="$((restore_count + 1))"
      done < <(jq -r '.imageDetails[]? | select(.imageStatus=="ARCHIVED") | .imageDigest' "${resp_json}" | sort -u)
    done
  fi
done < <(
  jq -c '
    sort_by(.account,.region,.repo)
    | group_by([.account,.region,.repo])
    | map({
        account: .[0].account,
        region: .[0].region,
        repo: .[0].repo,
        tags: (map(.tag) | map(select(. != null)) | unique),
        digests: (map(.digest) | map(select(. != null)) | unique)
      })
    | .[]
  ' "${targets_json}"
)

echo "Done. Restore operations triggered: ${restore_count}" >&2
