# E2E Reports Browser (Filestash)

Purpose: provide developers with a simple, authenticated web UI to explore and download E2E nightly HTML reports stored in S3.

## Scope

- Provisioned in all environments, but only **running in development** for now.
- In integration, staging, and production the ECS service desired count is set to `0`.

## URL and access flow

- URL: `https://e2e-reports.<public_domain>/`
- Authentication: enforced by the Tools ALB listener via **Cognito** before traffic reaches Filestash.
- Access control: approved users are managed in the shared Tools Cognito user pool and added to the `tools-filestash` group (see `docs/tools-users.md`).
- The Filestash admin UI route (`/admin*`) is blocked at the ALB listener level with a fixed 403 response.

## S3 location exposed

- Bucket (root): `${product}-${environment}-e2e-nightly-dev-reports-${account_id}`
- No narrower prefix is applied.

## Read-only enforcement

Read-only is enforced at IAM level via the Filestash ECS task role:

- `s3:ListBucket` on `arn:aws:s3:::${product}-${environment}-e2e-nightly-dev-reports-${account_id}`
- `s3:GetObject` on `arn:aws:s3:::${product}-${environment}-e2e-nightly-dev-reports-${account_id}/*`

Filestash community UI may still show upload/rename/delete controls depending on upstream behavior, but those operations must fail because IAM does not grant write/delete permissions.

If objects are encrypted with a customer-managed KMS key, the task role also includes a tightly-scoped `kms:Decrypt` permission restricted to S3 use for this bucket.

## Image pinning / updates

- Default image: `${orchestrator_account_id}.dkr.ecr.eu-west-2.amazonaws.com/cdp-e2e-reports:lowa@sha256:3ed5bf29eebe1a672124265bf1604a10e8c84ee9a58d7630ffad174c9d5b796a`
- Update process:
  1. Choose a stable upstream Filestash tag + digest.
  2. Mirror it to orchestrator ECR (see “Mirror to ECR (optional)”).
  3. Update `filestash_image` (tag + digest) in `modules/tools/variables.tf`.
  4. Run plans to confirm only the intended environments change.

### Mirror to ECR (optional)

If you prefer not to pull directly from Docker Hub in AWS, you can mirror the pinned upstream image to the **orchestrator account ECR**.
The repository is created by the existing orchestrator ECR component, named `cdp-e2e-reports`.

The Tools module defaults to using the orchestrator ECR image reference (tag + digest). If you need to override it, set the `filestash_image` input for the Tools module.

Example workflow (run in the **orchestrator** AWS account), following the same conventions as other tool images (for example CloudBeaver):

```sh
aws-switch-to-cdp-sirsi-orchestrator-goaco-terraform
ACCOUNT_ID=$(ave aws sts get-caller-identity | jq -r '.Account')

# Authenticate docker to ECR
ave aws ecr get-login-password --region eu-west-2 | docker login --username AWS --password-stdin ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com

# Pull the pinned upstream image
docker pull machines/filestash:lowa@sha256:3ed5bf29eebe1a672124265bf1604a10e8c84ee9a58d7630ffad174c9d5b796a

# Tag and push to ECR
docker tag machines/filestash:lowa@sha256:3ed5bf29eebe1a672124265bf1604a10e8c84ee9a58d7630ffad174c9d5b796a ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-e2e-reports:lowa

docker push ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-e2e-reports:lowa
```

## Notes / limitations

- Filestash is configured to use AWS credentials from the ECS task role (no static AWS keys).
- Multi-file HTML reports should be browsable and downloadable; rendering behavior depends on how the report references sibling assets (relative paths) and browser restrictions.
