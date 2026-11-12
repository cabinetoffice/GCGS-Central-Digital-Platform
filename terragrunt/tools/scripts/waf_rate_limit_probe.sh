#!/usr/bin/env bash
set -euo pipefail

BASE_URL="https://www.find-tender.service.gov.uk"
SLEEP_SECONDS="0.2"
DEFAULT_MAX_REQUESTS="120"
FOLLOW_REDIRECTS="0"
RUN="0"
LOG_DIR="/tmp"
GLOBAL_PROBE_PATHS="/Home/Cookies /Home/Privacy /cookies /privacy /help /about /robots.txt /sitemap.xml"

print_usage() {
  cat <<'USAGE'
Usage:
  waf_rate_limit_probe.sh [options] --profile <hightraffic|global|both>
  waf_rate_limit_probe.sh [options] --test "<name>|<path>|<expected_retry_after>|<expected_body_substring>|<max_requests>"
  waf_rate_limit_probe.sh [options]

Options:
  --base <url>        Base URL (default: https://www.find-tender.service.gov.uk)
  --sleep <seconds>   Sleep between requests (default: 0.2)
  --max <n>           Default max requests when not provided per-test (default: 120)
  --follow            Follow redirects (curl -L). Default: off
  --log-dir <dir>     Directory to write per-request logs (default: /tmp)
  --global-paths "<paths>" Space-separated candidate paths for the global probe (default: built-in list)
  --yes               Actually run. Without this, prints the planned tests only.

Profiles:
  hightraffic  /Search/Results expects Retry-After 120 and body containing "2 minutes"
  global       auto-picks a stable non-search page and expects Retry-After 300 and body containing "5 minutes"
  both         runs hightraffic then global (global will rate-limit your IP for ~5 minutes)

Default:
  If you provide no --profile/--test args, it runs the "both" profile.

Examples:
  tools/scripts/waf_rate_limit_probe.sh --profile hightraffic --yes
  tools/scripts/waf_rate_limit_probe.sh --profile global --yes
  tools/scripts/waf_rate_limit_probe.sh --base https://www.find-tender.service.gov.uk --test "global|/|300|5 minutes|90" --yes
USAGE
}

die() {
  echo "error: $*" >&2
  exit 2
}

trim_cr() {
  tr -d '\r'
}

curl_once() {
  local url="$1"
  local header_file="$2"
  local body_file="$3"

  : >"$header_file"
  : >"$body_file"
  : >"${body_file}.err"

  local -a curl_args
  curl_args=(-sS --compressed -D "$header_file" -o "$body_file" -w "%{http_code}" -H "Cache-Control: no-cache" -H "Pragma: no-cache")
  if [[ "$FOLLOW_REDIRECTS" == "1" ]]; then
    curl_args+=(-L)
  fi

  local code
  if ! code="$(curl "${curl_args[@]}" "$url" 2>"${body_file}.err" | trim_cr)"; then
    local msg
    msg="$(head -n 1 "${body_file}.err" 2>/dev/null | tr '\r\n' ' ' | sed -e 's/[[:space:]][[:space:]]*/ /g' -e 's/[[:space:]]*$//')"
    printf "curl_error: %s\n" "${msg:-unknown}" >"$body_file"
    echo "000"
    return 0
  fi

  echo "$code"
}

header_value() {
  local header_file="$1"
  local header_name="$2"
  awk -v h="${header_name,,}:" 'BEGIN{IGNORECASE=1} tolower($1)==h {print $2}' "$header_file" | head -n1 | tr -d '\r'
}

sanitize_one_line() {
  tr '\n' ' ' | tr -d '\r' | sed -e 's/[[:space:]][[:space:]]*/ /g' -e 's/[[:space:]]*$//'
}

resolve_global_probe_path() {
  local tmp_dir="$1"

  for candidate in $GLOBAL_PROBE_PATHS; do
    local url="${BASE_URL%/}${candidate}"
    local hdr="${tmp_dir}/resolve-headers.txt"
    local body="${tmp_dir}/resolve-body.txt"

    local status
    status="$(curl_once "$url" "$hdr" "$body")"
    local loc
    loc="$(header_value "$hdr" "location")"

    # Avoid dead paths, and avoid redirects to /Search (which is excluded from the global limiter).
    if [[ "$status" == "404" ]]; then
      continue
    fi
    if [[ "$status" =~ ^30[12378]$ && "$loc" =~ /[Ss]earch ]]; then
      continue
    fi

    echo "$candidate"
    return 0
  done

  return 1
}

body_contains() {
  local body_file="$1"
  local needle="$2"
  if [[ -z "$needle" ]]; then
    return 0
  fi
  grep -Fq -- "$needle" "$body_file"
}

run_test() {
  local name="$1"
  local path="$2"
  local expected_retry_after="$3"
  local expected_body_substring="$4"
  local max_requests="$5"

  local tmp_dir
  tmp_dir="$(mktemp -d)"
  trap 'rm -rf "$tmp_dir"' RETURN

  if [[ "$name" == "global" && "$path" == "AUTO" ]]; then
    local resolved
    if ! resolved="$(resolve_global_probe_path "$tmp_dir")"; then
      die "failed to auto-pick a global probe path; provide --test or set --global-paths"
    fi
    path="$resolved"
  fi

  local url="${BASE_URL%/}${path}"

  TEST_FIRST_429["$name"]="0"
  TEST_OUTCOME["$name"]="FAIL"
  TEST_LOG["$name"]=""

  echo
  echo "== $name =="
  echo "URL: $url"
  echo "Expect: first 429 with retry-after=${expected_retry_after} and body contains \"${expected_body_substring}\""
  echo "Max requests: $max_requests (sleep ${SLEEP_SECONDS}s)"

  mkdir -p "$LOG_DIR"
  local log_file="${LOG_DIR%/}/waf-rate-limit-probe-${name//[^a-zA-Z0-9._-]/_}-$(date +%Y%m%dT%H%M%S).tsv"
  printf "ts\treq\tstatus\tretry_after\tlocation\tbody_preview\n" >"$log_file"

  local first_429_at="0"
  local seen_status=""
  local seen_retry_after=""
  local seen_location=""
  local last_non_429_status=""
  local body_ok="0"
  local retry_ok="0"

  # If we're already rate-limited, we can't measure "how many requests until block".
  {
    local hdr0="${tmp_dir}/headers-0.txt"
    local body0="${tmp_dir}/body-0.txt"
    local status0
    status0="$(curl_once "$url" "$hdr0" "$body0")"
    local retry0
    retry0="$(header_value "$hdr0" "retry-after")"
    local loc0
    loc0="$(header_value "$hdr0" "location")"
    local body_preview0
    body_preview0="$(head -c 160 "$body0" | sanitize_one_line)"
    printf "%s\t%s\t%s\t%s\t%s\t%s\n" "$(date -Iseconds)" "0" "$status0" "${retry0:-}" "${loc0:-}" "$body_preview0" >>"$log_file"
    if [[ "$status0" == "429" ]]; then
      echo "Already rate-limited before the test started."
      echo "HTTP status: $status0"
      echo "Retry-After: ${retry0:-<missing>}"
      echo "Body preview: ${body_preview0:-<empty>}"
      echo "Log file: $log_file"
      echo "Wait for the Retry-After window to expire, then re-run to measure the threshold."
      TEST_LOG["$name"]="$log_file"
      return 1
    fi
  }

  declare -A status_counts=()
  for ((i=1; i<=max_requests; i++)); do
    local hdr="${tmp_dir}/headers-${i}.txt"
    local body="${tmp_dir}/body-${i}.txt"

    seen_status="$(curl_once "$url" "$hdr" "$body")"
    seen_retry_after="$(header_value "$hdr" "retry-after")"
    seen_location="$(header_value "$hdr" "location")"
    last_non_429_status="$seen_status"
    status_counts["$seen_status"]=$(( ${status_counts["$seen_status"]:-0} + 1 ))

    local body_preview
    body_preview="$(head -c 160 "$body" | sanitize_one_line)"
    printf "%s\t%s\t%s\t%s\t%s\t%s\n" "$(date -Iseconds)" "$i" "$seen_status" "${seen_retry_after:-}" "${seen_location:-}" "$body_preview" >>"$log_file"

    if [[ "$seen_status" == "429" ]]; then
      first_429_at="$i"

      if [[ -n "$expected_retry_after" && "$seen_retry_after" == "$expected_retry_after" ]]; then
        retry_ok="1"
      fi

      if body_contains "$body" "$expected_body_substring"; then
        body_ok="1"
      fi

      echo "Hit 429 at request #$first_429_at"
      echo "HTTP status: $seen_status"
      echo "Retry-After: ${seen_retry_after:-<missing>} (expected: ${expected_retry_after:-<any>})"
      echo "Body preview: ${body_preview:-<empty>}"
      echo "Checks: retry-after=$([[ "$retry_ok" == "1" ]] && echo OK || echo FAIL), body=$([[ "$body_ok" == "1" ]] && echo OK || echo FAIL)"
      echo "Log file: $log_file"
      TEST_LOG["$name"]="$log_file"
      break
    fi

    if [[ "$SLEEP_SECONDS" != "0" ]]; then
      sleep "$SLEEP_SECONDS"
    fi
  done

  if [[ "$first_429_at" == "0" ]]; then
    echo "Did not hit 429 within $max_requests requests (last status: ${last_non_429_status:-<none>})."
    if [[ "${last_non_429_status:-}" == "404" ]]; then
      echo "Hint: this path returned 404 for every request; try a known-good page path for global testing (e.g. /cookies, /help, /about) or provide --test."
    fi
    if [[ "${last_non_429_status:-}" =~ ^30[12378]$ ]]; then
      echo "Last Location: ${seen_location:-<missing>}"
      echo "Hint: you might be hitting a redirect-only edge path. Try a non-redirecting URL or add --follow."
    fi
    echo "Log file: $log_file"
    if [[ "${#status_counts[@]}" -gt 0 ]]; then
      echo "Status counts:"
      for k in "${!status_counts[@]}"; do
        echo "  $k: ${status_counts[$k]}"
      done
    fi
    TEST_LOG["$name"]="$log_file"
    return 1
  fi

  TEST_FIRST_429["$name"]="$first_429_at"

  if [[ -n "$expected_retry_after" && "$retry_ok" != "1" ]]; then
    return 1
  fi

  if [[ -n "$expected_body_substring" && "$body_ok" != "1" ]]; then
    return 1
  fi

  TEST_OUTCOME["$name"]="PASS"
  return 0
}

tests=()
add_test() {
  tests+=("$1")
}

declare -A TEST_OUTCOME
declare -A TEST_LOG
declare -A TEST_FIRST_429

profile=""
while [[ $# -gt 0 ]]; do
  case "$1" in
    --base)
      [[ $# -ge 2 ]] || die "--base requires a value"
      BASE_URL="$2"
      shift 2
      ;;
    --sleep)
      [[ $# -ge 2 ]] || die "--sleep requires a value"
      SLEEP_SECONDS="$2"
      shift 2
      ;;
    --max)
      [[ $# -ge 2 ]] || die "--max requires a value"
      DEFAULT_MAX_REQUESTS="$2"
      shift 2
      ;;
    --follow)
      FOLLOW_REDIRECTS="1"
      shift
      ;;
    --log-dir)
      [[ $# -ge 2 ]] || die "--log-dir requires a value"
      LOG_DIR="$2"
      shift 2
      ;;
    --global-paths)
      [[ $# -ge 2 ]] || die "--global-paths requires a value"
      GLOBAL_PROBE_PATHS="$2"
      shift 2
      ;;
    --yes)
      RUN="1"
      shift
      ;;
    --profile)
      [[ $# -ge 2 ]] || die "--profile requires a value"
      profile="$2"
      shift 2
      ;;
    --test)
      [[ $# -ge 2 ]] || die "--test requires a value"
      add_test "$2"
      shift 2
      ;;
    -h|--help)
      print_usage
      exit 0
      ;;
    *)
      die "unknown arg: $1 (use --help)"
      ;;
  esac
done

if [[ -z "$profile" && "${#tests[@]}" -eq 0 ]]; then
  profile="both"
fi

if [[ -n "$profile" ]]; then
  case "$profile" in
    hightraffic)
      add_test "hightraffic|/Search/Results|120|2 minutes|${DEFAULT_MAX_REQUESTS}"
      ;;
    global)
      add_test "global|AUTO|300|5 minutes|${DEFAULT_MAX_REQUESTS}"
      ;;
    both)
      add_test "hightraffic|/Search/Results|120|2 minutes|${DEFAULT_MAX_REQUESTS}"
      add_test "global|AUTO|300|5 minutes|${DEFAULT_MAX_REQUESTS}"
      ;;
    *)
      die "unknown profile: $profile"
      ;;
  esac
fi

if [[ "${#tests[@]}" -eq 0 ]]; then
  print_usage
  exit 2
fi

echo "Base URL: $BASE_URL"
echo "Follow redirects: $([[ "$FOLLOW_REDIRECTS" == "1" ]] && echo yes || echo no)"
echo "Planned tests:"
for t in "${tests[@]}"; do
  IFS='|' read -r name path expected_retry expected_body max_requests <<<"$t"
  max_requests="${max_requests:-$DEFAULT_MAX_REQUESTS}"
  echo "  - $name: ${path} (expect retry-after=${expected_retry}, body contains \"${expected_body}\", max=${max_requests})"
done

if [[ "$RUN" != "1" ]]; then
  echo
  echo "Dry-run only. Re-run with --yes to execute."
  exit 0
fi

overall_rc=0
for t in "${tests[@]}"; do
  IFS='|' read -r name path expected_retry expected_body max_requests <<<"$t"
  max_requests="${max_requests:-$DEFAULT_MAX_REQUESTS}"
  if ! run_test "$name" "$path" "$expected_retry" "$expected_body" "$max_requests"; then
    overall_rc=1
  fi
done

echo
echo "Summary:"
for t in "${tests[@]}"; do
  IFS='|' read -r name path expected_retry expected_body max_requests <<<"$t"
  echo "  - ${name}: ${TEST_OUTCOME[$name]:-FAIL} (first 429 at #${TEST_FIRST_429[$name]:-0}, log: ${TEST_LOG[$name]:-<none>})"
done
echo
if [[ "$overall_rc" == "0" ]]; then
  echo "RESULT: PASS"
else
  echo "RESULT: FAIL (see output above)"
fi

exit "$overall_rc"
