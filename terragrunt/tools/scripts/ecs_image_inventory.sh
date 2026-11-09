#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Build an inventory of container images referenced by ECS services (current + deployments).

Output: JSONL (one JSON object per line). By default includes only private ECR images.

Requirements:
  - aws cli v2, jq
  - IAM perms: ecs:ListClusters ecs:ListServices ecs:DescribeServices ecs:DescribeTaskDefinition

Usage:
  ecs_image_inventory.sh --region <aws-region> [--profile <profile>] [--clusters-regex <regex>] --out <file.jsonl> [--include-non-ecr]
EOF
}

region="${AWS_REGION:-${AWS_DEFAULT_REGION:-}}"
profile=""
clusters_regex=""
out=""
include_non_ecr="0"

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
    --out)
      out="${2:-}"; shift 2
      ;;
    --include-non-ecr)
      include_non_ecr="1"; shift
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
if [[ -z "${out}" ]]; then
  echo "Missing --out <file.jsonl>." >&2
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

echo "Region: ${region}" >&2
if [[ -n "${profile}" ]]; then echo "Profile: ${profile}" >&2; fi
if [[ -n "${clusters_regex}" ]]; then echo "Cluster filter regex: ${clusters_regex}" >&2; fi
echo "Out: ${out}" >&2
if [[ "${include_non_ecr}" == "1" ]]; then echo "Including non-ECR images: yes" >&2; else echo "Including non-ECR images: no" >&2; fi

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
  : > "${out}"
  exit 0
fi
echo "Clusters: ${cluster_count}" >&2

images_jsonl="${tmpdir}/images.jsonl"
: > "${images_jsonl}"

while IFS= read -r cluster_arn; do
  echo "Scanning cluster: ${cluster_arn}" >&2
  services_json="${tmpdir}/services.$(echo "${cluster_arn}" | tr '/:' '__').json"
  aws_cli ecs list-services --region "${region}" --cluster "${cluster_arn}" --output json > "${services_json}"
  mapfile -t service_arns < <(jq -r '.serviceArns[]?' "${services_json}")
  if [[ "${#service_arns[@]}" -eq 0 ]]; then
    continue
  fi

  for ((i=0; i<${#service_arns[@]}; i+=10)); do
    chunk=("${service_arns[@]:i:10}")
    desc_json="${tmpdir}/describe-services.$RANDOM.json"
    aws_cli ecs describe-services --region "${region}" --cluster "${cluster_arn}" --services "${chunk[@]}" --output json > "${desc_json}"

    # Emit (clusterArn, serviceArn, taskDefinitionArn) tuples (include deployments taskdefs too).
    tuples_jsonl="${tmpdir}/tuples.$RANDOM.jsonl"
    jq -c --arg cluster "${cluster_arn}" '
      .services[]
      | .serviceArn as $svc
      | ([.taskDefinition] + ((.deployments // []) | map(.taskDefinition)) | unique)
      | .[]?
      | {clusterArn: $cluster, serviceArn: $svc, taskDefinitionArn: .}
    ' "${desc_json}" > "${tuples_jsonl}"

    while IFS= read -r tup; do
      td_arn="$(jq -r '.taskDefinitionArn' <<<"${tup}")"
      td_json="${tmpdir}/taskdef.$RANDOM.json"
      aws_cli ecs describe-task-definition --region "${region}" --task-definition "${td_arn}" --output json > "${td_json}"
      jq -c --argjson meta "${tup}" '
        .taskDefinition.containerDefinitions[]
        | $meta + {containerName: .name, image: .image}
      ' "${td_json}" >> "${images_jsonl}"
    done < "${tuples_jsonl}"
  done
done < <(jq -r '.[]' "${clusters_filtered_json}")

# Normalize and (by default) filter to private ECR images.
jq -c --arg include_non_ecr "${include_non_ecr}" '
  . as $row
  | .image as $img
  | (
      try ($img | capture("^(?<account>[0-9]{12})\\.dkr\\.ecr\\.(?<region>[a-z0-9-]+)\\.amazonaws\\.com\\/(?<repo>[^:@]+)(?:(?::(?<tag>[^@]+))|(?:@(?<digest>sha256:[0-9a-f]{64})))$")) catch null
    ) as $p
  | if ($p == null) then
      if ($include_non_ecr == "1") then
        $row + {ecr: false}
      else
        empty
      end
    else
      $row + {
        ecr: true,
        account: $p.account,
        repo: $p.repo,
        imageRegion: $p.region,
        tag: ($p.tag // null),
        digest: ($p.digest // null)
      }
    end
' "${images_jsonl}" \
| jq -cs 'unique_by([.clusterArn,.serviceArn,.taskDefinitionArn,.containerName,.image])[]' \
> "${out}"

echo "Wrote inventory lines: $(wc -l < "${out}" | tr -d ' ')" >&2
