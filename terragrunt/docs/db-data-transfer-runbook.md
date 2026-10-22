# Database Data Transfer Runbook (Source -> Target)

Use this runbook to move database data between AWS accounts/environments.
It is intentionally generic so it can cover prod->staging, prod->dev, or any source->target pair.

## Scope
- Works for source and target accounts/environments.
- Supports dump/restore or snapshot-based restores (choose one path below).
- Covers validation and post-restore steps.

## Before You Start (fill these in)
- Source account/env: `<source-account>/<source-env>`
- Target account/env: `<target-account>/<target-env>`
- Database engine: `<postgres|mysql|...>`
- Database name: `<db-name>`
- Source DB identifier/cluster: `<source-db-identifier>`
- Target DB identifier/cluster: `<target-db-identifier>`
- Credentials / access method: `<aws-vault profile or other>`
- Data sensitivity actions (PII scrub, user/password rotation, etc.): `<required steps>`
- Change window / approvals: `<ticket or approval link>`

## Option A: Snapshot Restore (preferred when possible)
1) Create a snapshot in the source account.
   - Note the snapshot ID: `<snapshot-id>`
2) Share/copy the snapshot to the target account (if cross-account).
3) Restore snapshot into target account/environment.
4) Update target secrets/parameters to point to the restored instance/cluster.
5) Run DB migrations (if required).
6) Validate the restore (see Validation below).

## Option B: Dump/Restore (when snapshots are not feasible)
1) Create a dump from the source DB (tool depends on engine).
2) Transfer the dump to the target environment (S3 or import host).
3) Restore into the target DB.
4) Run DB migrations (if required).
5) Validate the restore (see Validation below).



## KMS Key (CLI) for Cross-Account RDS Snapshot Copy
Use this when you need a new key that the target account can use for RDS snapshot copy.
Fill in placeholders before running.

### 1) Create the key
```shell
aws kms create-key   --description "SIRSI RDS snapshot copy key (source->target)"   --key-usage ENCRYPT_DECRYPT   --origin AWS_KMS   --tags TagKey=Project,TagValue=SIRSI TagKey=Purpose,TagValue=RDS-Snapshot-Copy TagKey=Environment,TagValue=<source-env>
```

### 2) Add an alias
```shell
aws kms create-alias   --alias-name alias/sirsi/rds-snapshot-copy/<source-env>   --target-key-id <key-id>
```

### 3) Apply the key policy
```shell
aws kms put-key-policy   --key-id <key-id>   --policy-name default   --policy file://kms-snapshot-copy-policy.json
```

### 4) Verify
```shell
aws kms describe-key --key-id <key-id>
```

## SIRSI MySQL Notes (example flow)
This is the observed MySQL flow used for a source->target restore.
Adjust names/paths per environment.

1) Create an RDS snapshot in the source account using a KMS key that grants access to the target account.
2) Share the snapshot with the target account (if cross-account).
3) In the target account, set the snapshot name in `root.hcl` so Terragrunt restores from that snapshot.
4) Run `terragrunt apply` for the relevant DB module/component to trigger the restore.
5) Assume `terragrunt apply` will recreate/restore the DB from the snapshot.
   - If it does not, document the required manual steps and update this runbook.

## Validation
- Check row counts for key tables.
- Spot-check recent records.
- Run application smoke tests.
- Confirm background jobs and integrations behave as expected.

## Post-restore Tasks
- Rotate/confirm credentials and secrets.
- Disable or reconfigure outbound integrations if needed in non-prod.
- Update environment configs referencing DB endpoints.
- Record completion and any exceptions.

## References
- FTS import EC2 setup: `docs/fts-db-import.md`
- Secrets management: `docs/manage-secrets.md`


## Deletion Protection / Final Snapshot Notes (Staging)
- Staging RDS clusters may still have deletion protection or final snapshot requirements depending on module settings.
- If a destroy fails due to final snapshot requirements, ensure `skip_final_snapshot` is true for staging in Terraform or delete manually with `--skip-final-snapshot`.
- If deletion protection blocks a destroy, disable it first (AWS Console or CLI) before re-running apply.


## Automated weekly prod->staging refresh (FTS)
This repo provisions a weekly refresh for the **FTS Aurora MySQL** cluster from production to staging.

- Scheduler: Thursday 21:21 Europe/London
- Orchestrator pipeline: `cdp-sirsi-fts-db-weekly-refresh`
- Staging snapshot SSM parameter: `/cdp-sirsi/staging/fts/weekly_refresh_snapshot_id`

During testing, the pipeline can run in **scratch** mode to avoid impacting the real staging DB/services:
- Scratch cluster id: `cdp-sirsi-fts-cluster-refresh-test`
- Scratch mode skips the Terragrunt restore, Step Functions migrations, and ECS redeploys
- Switch back to **main** mode to restore the real staging cluster

If the SSM parameter is set to a snapshot identifier, the staging DB module restores from it.

**Main mode is destructive:** it deletes the existing staging FTS cluster (`cdp-sirsi-fts-cluster`) and recreates it from the copied production snapshot so the restored cluster keeps the same identifier. Expect downtime during the delete/restore.


### Staging post-restore password reset (CloudBeaver/tool users)
After the staging FTS cluster is restored from the production snapshot, the pipeline runs SQL to reset internal DB user passwords so tools (e.g., CloudBeaver) keep working.

It reads the passwords from this staging secret:
- Secret name: `rds-db-cdp-sirsi-fts-cluster-tool-user-passwords`
- Secret JSON keys: `cb_ro_fts`, `cb_da_fts`

Example secret value:
```json
{
  "cb_ro_fts": "<password>",
  "cb_da_fts": "<password>"
}
```

The SQL executed is:
```sql
ALTER USER 'cb_ro_fts'@'%' IDENTIFIED BY '<from secret>';
ALTER USER 'cb_da_fts'@'%' IDENTIFIED BY '<from secret>';
GRANT CREATE USER, ALTER USER ON *.* TO 'cb_da_fts'@'%';
GRANT SELECT ON mysql.* TO 'cb_da_fts'@'%';
ALTER USER 'cdp_sirsi_fts_cluster_migrator'@'%' IDENTIFIED BY '<RUN_MIGRATOR_TOKEN from cdp-sirsi-fts/secrets>';
FLUSH PRIVILEGES;
```

> TBC: More queries to be identified and executed to reestablish user-organisation links allowing successful login 