# FTS DB Import EC2 Setup Guide

This guide explains how to prepare the import EC2 instance for uploading large database dump files, mounting the persistent EBS volume, and installing the MySQL client.

---

## 1. SSH into the EC2 instance
*Please obtain the private SSH key from DevOps before attempting to connect.*

From your local terminal:

```shell

ssh -i ~/.ssh/fts_db_import.pem ubuntu@fts-import.<env-name>.supplier-information.find-tender.service.gov.uk
```

---

## 2. Mount the EBS data volume

> Note: the persistent EBS data volume is no longer provisioned by default (all environments). The import host still exists, but `/dev/nvme1n1` and `/mnt/import_data` will not exist unless the disk is explicitly re-enabled.

### Option A (default): No persistent import disk
If you just need to run an ad-hoc import, use the instance root volume (space permitting) or download the dump from S3 on-demand.

Example (root volume):
```shell
df -h
mkdir -p ~/dumps
```

Upload to the instance home directory:
```shell
scp -i ~/.ssh/fts_db_import.pem /path/to/your/large-dump.sql ubuntu@fts-import.<env-name>.supplier-information.find-tender.service.gov.uk:~/dumps/
```

### Option B: Re-enable the import EBS volume (temporary)
If you need a large persistent disk for the import workflow, re-enable it for the target environment and apply the database component, then mount it.

1) In this repo, set `import_data_disk_enabled = true` for the target environment in `components/root.hcl`
2) Apply the DB component for that environment (e.g. `components/service/database`)
3) Run the following commands inside the EC2 instance **if it’s the first time**:

```shell

# Prepare Storage
lsblk
sudo mkfs.ext4 /dev/nvme1n1
sudo mkdir -p /mnt/import_data
sudo mount /dev/nvme1n1 /mnt/import_data
df -h /mnt/import_data
sudo chown ubuntu:ubuntu /mnt/import_data
echo '/dev/nvme1n1 /mnt/import_data ext4 defaults,nofail 0 2' | sudo sudo tee -a /etc/fstab
mkdir -p /mnt/import_data/dumps

# Install MySQL client on the EC2 instance
sudo apt update
sudo apt install -y mysql-client
# Verify installation:
mysql --version
```
---

## 3. Upload your dump file from your local machine

Use scp with the same key you used for SSH:
```shell
scp -i ~/.ssh/fts_db_import.pem /path/to/your/large-dump.sql ubuntu@fts-import.<env-name>.supplier-information.find-tender.service.gov.uk:~/dumps/

```
If you have re-enabled the EBS import disk, you can upload to the mounted path instead:
```shell
scp -i ~/.ssh/fts_db_import.pem /path/to/your/large-dump.sql ubuntu@fts-import.<env-name>.supplier-information.find-tender.service.gov.uk:/mnt/import_data/dumps/
```
