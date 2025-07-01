#!/usr/bin/env python3

import boto3
import os
import tempfile
import subprocess
import argparse
import sys
import csv
from datetime import datetime

s3 = boto3.client('s3')

def list_zero_byte_files(bucket):
    paginator = s3.get_paginator('list_objects_v2')
    zero_files = []

    for page in paginator.paginate(Bucket=bucket):
        for obj in page.get('Contents', []):
            if obj['Size'] == 0:
                zero_files.append(obj['Key'])

    return zero_files

def download_file(bucket, key, dest_path):
    s3.download_file(bucket, key, dest_path)

def scan_with_clamav(file_path):
    try:
        result = subprocess.run(
            ['docker', 'exec', 'clamav', 'clamscan', file_path],
            stdout=subprocess.PIPE, stderr=subprocess.PIPE, check=True
        )
        output = result.stdout.decode()
        return "Infected files: 0" in output
    except subprocess.CalledProcessError as e:
        print(f"❌ ClamAV scan failed: {e.stderr.decode()}")
        return False

def upload_file(bucket, key, file_path):
    s3.upload_file(file_path, bucket, key)

def process_files(target_bucket, temps_bucket, do_overwrite):
    zero_files = list_zero_byte_files(target_bucket)
    print(f"\n🔍 Found {len(zero_files)} zero-byte files in '{target_bucket}'\n")

    clean_files = []
    report_rows = []

    for key in zero_files:
        with tempfile.NamedTemporaryFile(delete=False) as tmp:
            local_path = tmp.name
        try:
            print(f"📥 Downloading '{key}' from '{temps_bucket}'...")
            try:
                download_file(temps_bucket, key, local_path)
            except s3.exceptions.ClientError as e:
                if e.response['Error']['Code'] == '404':
                    report_rows.append([key, "missing", "Not found in staging bucket"])
                    print(f"❌ Missing: {key}")
                    continue
                else:
                    raise

            print(f"🦠 Scanning '{key}'...")
            if scan_with_clamav(local_path):
                clean_files.append((key, local_path))
                report_rows.append([key, "clean", "Ready to upload"])
                print(f"✅ Clean: {key}")
            else:
                report_rows.append([key, "infected", "Virus found by ClamAV"])
                print(f"🚫 Infected: {key}")
        except Exception as e:
            report_rows.append([key, "error", f"{type(e).__name__}: {e}"])
            print(f"⚠️ Error processing {key}: {e}")
        finally:
            pass  # Delay removing local file in case of upload

    if not do_overwrite:
        print(f"\n📝 Dry-run complete. {len(clean_files)} clean files ready to overwrite.\n")
        for key, _ in clean_files:
            print(f" - {key}")
    else:
        print(f"\n🚀 Starting overwrite of {len(clean_files)} clean files...")
        for key, local_path in clean_files:
            try:
                upload_file(target_bucket, key, local_path)
                print(f"⬆️ Uploaded: {key}")
            except Exception as e:
                print(f"❌ Upload failed for {key}: {e}")
            finally:
                os.remove(local_path)

    # Write report
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    report_path = f"reprocess_report_{timestamp}.csv"
    with open(report_path, "w", newline="") as csvfile:
        writer = csv.writer(csvfile)
        writer.writerow(["filename", "status", "message"])
        writer.writerows(report_rows)
    print(f"\n📄 Report written to: {report_path}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Recover and rescan zero-byte S3 uploads.")
    parser.add_argument("--target-bucket", required=True, help="Target bucket where zero-byte files exist.")
    parser.add_argument("--temps-bucket", required=True, help="Staging bucket with full-size files.")
    parser.add_argument("--overwrite", action="store_true", help="Actually upload clean files to target bucket.")

    args = parser.parse_args()

    if args.target_bucket == args.temps_bucket:
        print("❌ Error: Target and temps bucket must be different.")
        sys.exit(1)

    process_files(args.target_bucket, args.temps_bucket, args.overwrite)
