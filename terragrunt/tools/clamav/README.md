# ClamAV (fts-app sidecar)

The official [clamav/clamav image](https://hub.docker.com/r/clamav/clamav) with [clamd.conf](clamd.conf) built in. It runs as
the `clamav` sidecar container in the `fts-app` ECS task, where the FTS .NET frontend scans
uploaded documents by streaming them to `127.0.0.1:3310` (clamd `INSTREAM`).

The sidecar is enabled per environment with `fts_clamav_enabled` in `components/root.hcl`. Its size
is set in `fts_clamav_sidecar_sizes`, and the image tag in `fts_clamav_image`, both in
`modules/ecs/locals-fts.tf`.

- **Signatures** ship with the image and are kept up to date at runtime by freshclam inside the
  container (`FRESHCLAM_CHECKS=12`, every 2 hours). A new image is not needed to get new signatures.
- **Engine updates** need a rebuild. The Dockerfile follows `clamav/clamav:stable`, but Terraform
  deploys an explicit tag, so a new image only reaches an environment when that tag is changed.
  Don't deploy `latest`: clamd is an essential container that fts-app waits on, so a clamd image
  that fails to start stops fts-app from starting.

> In the following examples ave is alias for `aws-vault exec` command.
Feel free to use any convenient AWS profiler instead.

## Build

Fargate runs `linux/amd64`, so build for that platform (needed on Apple Silicon).

```shell
docker build --pull --platform linux/amd64 -t cabinetoffice/cdp-clamav:build .
docker run --rm --platform linux/amd64 cabinetoffice/cdp-clamav:build clamd --version
# ClamAV 1.5.5/<signature version>/<date>
```

Tag the image `<engine version>-<build number>`. Use the engine version from `clamd --version`, and
start the build number at `1`. Increase the build number when only `clamd.conf` changes, and reset
it to `1` for a new engine version. The ECR repository is mutable, so never reuse a tag that has
already been pushed.

```shell
VERSION=1.5.5-1
docker tag cabinetoffice/cdp-clamav:build cabinetoffice/cdp-clamav:${VERSION}
```

## Deploy

### Push to ECR

The `cdp-clamav` ECR repository is in the orchestrator account.

```shell
aws-switch-to-cdp-sirsi-orchestrator-goaco-terraform
ACCOUNT_ID=$(ave aws sts get-caller-identity | jq -r '.Account')
ave aws ecr get-login-password --region eu-west-2 | docker login --username AWS --password-stdin ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com
docker tag cabinetoffice/cdp-clamav:${VERSION} ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-clamav:${VERSION}
docker push ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-clamav:${VERSION}
```

### Roll out

Set the new tag in `fts_clamav_image` (`modules/ecs/locals-fts.tf`), then plan and apply
`components/service/ecs`, starting with development. fts-app gets a new task definition, and its
rolling deployment keeps the old tasks serving until the new ones are healthy.

## Verify

After deploying, upload a document to a notice in the environment:

- A clean file uploads, and the `/ecs/fts-app` log group shows
  `ClamAV scanned uploaded file <name>: clean`. The `clamav/...` log streams show
  `stream: OK`.
- The [EICAR test file](https://www.eicar.org/download-anti-malware-testfile/) is rejected as a
  virus, and the logs show `Win.Test.EICAR_HDB-1 FOUND`.

If clamd is unreachable, every upload is rejected (fail-closed), and the
`cdp-sirsi-<env>-fts-clamav-scan-failed` alarm goes to the `cdp-sirsi-<env>-alerts` topic.
