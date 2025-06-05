#!/usr/bin/env python3

import boto3
import argparse
import datetime

parser = argparse.ArgumentParser(description='Generate pre-signed URLs for both upload and download of an S3 object.')
parser.add_argument('--bucket', required=True, help='Target S3 bucket name')
parser.add_argument('--key', required=True, help='Base key prefix (e.g. dumps/db_backup)')
parser.add_argument('--expires', default=3600, type=int, help='Expiration time in seconds (default: 3600)')

args = parser.parse_args()

timestamp = datetime.datetime.utcnow().strftime('%Y%m%d_%H%M%S')
object_key = f"{args.key}_{timestamp}.sql"

s3 = boto3.client("s3", region_name="eu-west-2", endpoint_url="https://s3.eu-west-2.amazonaws.com")

upload_url = s3.generate_presigned_url(
    ClientMethod='put_object',
    Params={'Bucket': args.bucket, 'Key': object_key},
    ExpiresIn=args.expires
)

download_url = s3.generate_presigned_url(
    ClientMethod='get_object',
    Params={'Bucket': args.bucket, 'Key': object_key},
    ExpiresIn=args.expires
)


print(f"\n✅ Pre-signed URLs generated for object key:\n  {object_key}\n")

print("📁 Set your local file path:")
print("export file_to_upload=</your/path/to/file.sql>")

print("\n📦 To upload your file, run:\n")
print(f'curl --location --request PUT \\')
print(f'     --upload-file "$file_to_upload" \\')
print(f'     "{upload_url}"')

print("\n📥 To download the uploaded file, run:\n")
print(f'curl --location --request GET \\')
print(f'     --output downloaded_{timestamp}.sql \\')
print(f'     "{download_url}"\n')
