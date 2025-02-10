# Grafana K6

This project provides a **Dockerised setup** for running [Grafana K6](https://hub.docker.com/r/grafana/k6) performance tests.  
It includes all K6 test scripts from the `./scripts` directory and is ready to be initiated as an **ECS task** in AWS.


> In the following examples ave is alias for `aws-vault exec` command.
Feel free to use any convenient AWS profiler instead.

## build

```shell
docker build -t cabinetoffice/cdp-k6 .
```

## Run locally

Use the following command to execute a K6 test script (e.g., organisation-lookup.js) against the development environment:

```shell
docker run --rm \
  -e AUTH_TOKEN="<your_real_OTP_here>" \
  -e DURATION=10s \
  -e MAX_VUS=100 \
  -e RPS=15 \
  -e TARGET_DOMAIN="dev.supplier.information.findatender.codatt.net" \
  -e VUS=20 \
  cabinetoffice/cdp-k6 run /scripts/organisation-lookup.js

```

## Deploy

### Push to ECR

There is ECR repositories in orchestrator account. Using the following commands, we can push the built image.

```shell
aws-switch-to-cdp-sirsi-orchestrator-goaco-terraform
ACCOUNT_ID=$(ave aws sts get-caller-identity | jq -r '.Account')
ave aws ecr get-login-password --region eu-west-2 | docker login --username AWS --password-stdin ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com
docker tag cabinetoffice/cdp-k6:latest ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-k6:latest
docker push ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-k6:latest
```

### Initiate Grafana K6 service

```shell
ave aws ecs update-service --cluster cdp-sirsi --service k6 --force-new-deployment | jq -r '.'

```
