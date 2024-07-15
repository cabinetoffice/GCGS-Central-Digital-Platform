# Coldbuild

This configuration is to build custom CodeBuild image to be used by CodeBuild jobs in different accounts.

> In the following examples ave is alias for `aws-vault exec` command.
Feel free to use any convenient AWS profiler instead.

## build

```shell
TERRAFORM_VERSION=$(grep -Po '(?<=terraform = ")[^"]*' ../../../.mise.toml)
TERRAGRUNT_VERSION=$(grep -Po '(?<=terragrunt = ")[^"]*' ../../../.mise.toml)

if [ -z "$TERRAFORM_VERSION" ] || [ -z "$TERRAGRUNT_VERSION" ]; then
  echo "Error: Could not find Terraform or Terragrunt versions in .mise.toml"
  exit 1
fi

docker build --build-arg TERRAFORM_VERSION=$TERRAFORM_VERSION\
             --build-arg TERRAGRUNT_VERSION=$TERRAGRUNT_VERSION \
             -t cabinetoffice/cdp-codebuild .

echo -e "Built completed, and the new image contains:\n $(docker run --rm cabinetoffice/cdp-codebuild terraform version)\n $(docker run --rm cabinetoffice/cdp-codebuild terragrunt --version)\n$(docker run --rm cabinetoffice/cdp-codebuild aws --version)"

```

## Deploy

### Push to ECR

There are individual ECR repositories in each account. Using the following commands, we can push the built image to different accounts.

```shell
ACCOUNT_ID=$(ave aws sts get-caller-identity | jq -r '.Account')
ave aws ecr get-login-password --region eu-west-2 | docker login --username AWS --password-stdin ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com
docker tag cabinetoffice/cdp-codebuild:latest ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-codebuild:latest
docker push ${ACCOUNT_ID}.dkr.ecr.eu-west-2.amazonaws.com/cdp-codebuild:latest
```
