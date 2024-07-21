# Coldbuild

This configuration is to build custom CodeBuild image to be used by CodeBuild jobs in different accounts.

> In the following examples ave is alias for `aws-vault exec` command. aws-switch-to-* commands are custom functions to set profiler.\
Feel free to use any convenient AWS profiler instead.

## build

```shell
pushd ../../
./tools/scripts/delete_tf_cache.sh
TERRAFORM_VERSION=$(grep -Po '(?<=terraform = ")[^"]*' ../.mise.toml)
TERRAGRUNT_VERSION=$(grep -Po '(?<=terragrunt = ")[^"]*' ../.mise.toml)

if [ -z "$TERRAFORM_VERSION" ] || [ -z "$TERRAGRUNT_VERSION" ]; then
  echo "Error: Could not find Terraform or Terragrunt versions in .mise.toml"
  exit 1
fi

docker build --build-arg TERRAFORM_VERSION=$TERRAFORM_VERSION\
             --build-arg TERRAGRUNT_VERSION=$TERRAGRUNT_VERSION \
             -t cabinetoffice/cdp-codebuild \
             -f ./tools/codebuild/Dockerfile .

echo -e "Built completed, and the new image contains:"
docker run --rm cabinetoffice/cdp-codebuild sh -c 'echo "{\"AWS\": \"`aws --version| head -n 1`\", \"JQ \": \"`jq --version`\", \"TF \": \"`terraform version| head -n 1`\", \"TG \": \"`terragrunt --version`\"}" | jq .' 

popd 
```

## Deploy

### Push to ECR

There is ECR repositories in orchestrator account. Using the following commands, we can push the built image.

```shell
aws-switch-to-cdp-sirsi-orchestrator-goaco-terraform
ACCOUNT_ID=$(ave aws sts get-caller-identity | jq -r '.Account')
ave aws ecr get-login-password --region eu-west-2 | docker login --username AWS --password-stdin ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com
docker tag cabinetoffice/cdp-codebuild:latest ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-codebuild:latest
docker push ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-codebuild:latest
```
