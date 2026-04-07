# ACM + LetsEncrypt automation (HTTP-01 via ALB fixed-response)

This folder adds a safe, temporary ALB listener rule to satisfy HTTP-01
validation without changing app env vars. Existing manual flow remains untouched.

## Prereqs
- certbot installed
- aws CLI configured with access to ELBv2 + ACM
- aws-vault alias `ave` available (preferred) or set AWS_CMD to your wrapper

## How it works
- certbot runs in manual mode with auth/cleanup hooks
- auth hook creates a temporary listener rule for:
    - host header: your domain
    - path: /.well-known/acme-challenge/<token>
    - fixed response body: <token>.<key-authorization>
- cleanup hook deletes the rule
- certificate is imported to ACM using `terragrunt/tools/manual-certs/letsencrypt.sh`

## Run (short command)
From this folder:
- `ave ./rotate-certs.sh staging`
- `ave ./rotate-certs.sh integration`
- `ave ./rotate-certs.sh production`
Attach to HTTPS listener as part of the run:
- `ave ./rotate-certs.sh staging --attach`

## Attach to HTTPS listener (SNI)
From this folder:
- `ave ./attach-certs.sh staging`
- `ave ./attach-certs.sh integration`
- `ave ./attach-certs.sh production`

## Dry run (preflight)
Resolves domains + listener ARNs only (no certbot, no ACM import):
- `ave ./rotate-certs.sh staging --dry-run`
Resolves domains + ACM cert ARNs + HTTPS listener ARNs:
- `ave ./rotate-certs.sh staging --dry-run --attach`
Resolves domains + ACM cert ARNs + HTTPS listener ARNs:
- `ave ./attach-certs.sh staging --dry-run`

## Validate certs (ACM + live)
From this folder (manual before/after checks):
- `ave ./validate-certs.sh staging`
- `ave ./validate-certs.sh staging --dry-run`

## rotate-certs.sh
`rotate-certs.sh <env>` issues **both** FTS and CFS certs for the given env by
reading `domains.env` in this folder.

Defaults:
- FTS_LB_NAME = cdp-sirsi-php
- CFS_LB_NAME = cdp-sirsi-php
- EMAIL = ali.bahman@goaco.com
- VERIFY_CHALLENGE = 1
- ACME_SERVER = (default: Let's Encrypt production)
  - production: `https://acme-v02.api.letsencrypt.org/directory`
  - staging: `https://acme-staging-v02.api.letsencrypt.org/directory`
  - docs: `https://letsencrypt.org/docs/staging-environment/`
- KEY_TYPE = rsa
- RSA_KEY_SIZE = 2048

Flags:
- `--dry-run` (preflight only)
- `--attach` (attach SNI certs after import)

Common env overrides:
- `AWS_REGION` (default eu-west-2)
- `AWS_CMD` (e.g. `AWS_CMD="ave aws"`)
- `PYTHON_BIN` (python interpreter)
- `DOMAINS_FILE` (default `domains.env`)

rotate-certs env overrides:
- `FTS_LB_NAME` / `CFS_LB_NAME`
- `EMAIL`
- `VERIFY_CHALLENGE` (default 1)
- `KEEP_RULE` (set to 1 to retain ALB rule)
- `ACME_SERVER` (override CA directory URL; see above)
- `KEY_TYPE` (default rsa)
- `RSA_KEY_SIZE` (default 2048)

issue-cert env overrides:
- `DOMAIN` (single domain) or `DOMAINS` (comma-separated)
- `LOAD_BALANCER_NAME` (if LISTENER_ARN not set)
- `LISTENER_ARN` (HTTP:80 listener ARN)
- `STATE_DIR` / `CERT_DIR` / `CERTBOT_DIR`
- `CERT_ARN_FILE` (write imported ARN here)

attach-certs env overrides:
- `CERT_ARN_FTS` / `CERT_ARN_CFS` (attach these ARNs directly)
- `FTS_LB_NAME` / `CFS_LB_NAME`

validate-certs env overrides:
- `CERT_ARN_FTS` / `CERT_ARN_CFS` (use these ARNs for ACM details)
- `LOG_FILE` (write output to file)
- `LIVE_WAIT_SECONDS` (default 120)
- `LIVE_WAIT_INTERVAL` (default 5)
- `SUMMARY_FILE` (write env summary to file)
- `PREV_LIVE_SERIAL_FTS` / `PREV_LIVE_SERIAL_CFS` (optional change detection)
- `PREV_LIVE_NOT_AFTER_FTS` / `PREV_LIVE_NOT_AFTER_CFS` (optional change detection)


## Notes
- The temporary rule only affects the single token path.
- Priorities are chosen automatically and retried if colliding.
- State is stored under `terragrunt/tools/manual-certs/work/manual-certs-<env>-*` when using rotate-certs.
