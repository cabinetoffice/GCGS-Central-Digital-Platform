data "aws_iam_policy_document" "terraform_global_efs" {

  statement {
    actions = [
      "ec2:AttachNetworkInterface",
      "ec2:CreateNetworkInterface",
      "elasticfilesystem:CreateAccessPoint",
      "elasticfilesystem:DescribeAccessPoints",
      "elasticfilesystem:DeleteAccessPoint",
      "elasticfilesystem:CreateFileSystem",
      "elasticfilesystem:CreateMountTarget",
      "elasticfilesystem:DeleteFileSystem",
      "elasticfilesystem:DeleteMountTarget",
      "elasticfilesystem:DescribeFileSystems",
      "elasticfilesystem:DescribeLifecycleConfiguration",
      "elasticfilesystem:DescribeMountTargetSecurityGroups",
      "elasticfilesystem:DescribeMountTargets",
      "elasticfilesystem:TagResource",
      "elasticfilesystem:UntagResource",
    ]
    effect = "Allow"
    resources = [
      "*",
    ]
    sid = "ManageEFS"
  }

}
