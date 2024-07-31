FROM amazonlinux:latest

# Build arguments for Terraform and Terragrunt versions
ARG TERRAFORM_VERSION
ARG TERRAGRUNT_VERSION

# Install yum-config-manager to manage your repositories
RUN yum install -y yum-utils

# Use yum-config-manager to add the official HashiCorp Linux repository
RUN yum-config-manager --add-repo https://rpm.releases.hashicorp.com/AmazonLinux/hashicorp.repo

# Install Terraform
RUN yum install -y terraform-${TERRAFORM_VERSION}

# Install necessary tools
RUN yum install -y wget unzip jq

# Download and install Terragrunt
RUN wget https://github.com/gruntwork-io/terragrunt/releases/download/v${TERRAGRUNT_VERSION}/terragrunt_linux_amd64
RUN mv terragrunt_linux_amd64 /usr/local/bin/terragrunt
RUN chmod +x /usr/local/bin/terragrunt



# Install AWS CLI
RUN curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
RUN unzip awscliv2.zip
RUN ./aws/install

# Verify tools are installed
RUN terraform --version
RUN terragrunt --version
RUN aws --version
RUN jq -V
