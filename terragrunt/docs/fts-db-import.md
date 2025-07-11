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

Run the following commands inside the EC2 instance **if it’s the first time**:

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
scp -i ~/.ssh/fts_db_import.pem /path/to/your/large-dump.sql ubuntu@fts-import.<env-name>.supplier-information.find-tender.service.gov.uk:/mnt/import_data/dumps/

```
This will transfer your dump file directly into your persistent volume.
