resource "tls_private_key" "import_key" {
  algorithm = "RSA"
  rsa_bits  = 2048
}

resource "aws_key_pair" "import_key" {
  key_name   = "fts-db-import"
  public_key = tls_private_key.import_key.public_key_openssh
}

resource "aws_instance" "fts_db_import" {
  count = local.has_import_instance ? 1 : 0

  ami                     = "ami-028a245ba118f4c66"
  disable_api_stop        = false
  disable_api_termination = true
  instance_type           = "c7i.2xlarge"
  subnet_id               = var.public_subnet_ids[0]

  key_name = aws_key_pair.import_key.key_name

  vpc_security_group_ids = [var.ec2_sg_id]

  tags = local.import_instance_tags
}

resource "aws_ec2_instance_state" "fts_db_import" {
  count       = local.has_import_instance ? 1 : 0
  instance_id = aws_instance.fts_db_import[0].id
  state       = local.import_instance_state
}

resource "aws_ebs_volume" "import_data_disk" {
  count = local.has_import_instance ? 1 : 0

  availability_zone = data.aws_subnet.first_public_subnet.availability_zone
  iops              = 64000
  size              = 1280
  type              = "io1"


  tags = local.import_instance_tags
}

resource "aws_volume_attachment" "import_data_attach" {
  count = local.has_import_instance ? 1 : 0

  device_name = "/dev/sdf" # Will show up as /dev/xvdf
  volume_id   = aws_ebs_volume.import_data_disk[0].id
  instance_id = aws_instance.fts_db_import[0].id
}

resource "aws_eip" "fts_db_import" {
  count = local.has_import_instance ? 1 : 0

  tags = local.import_instance_tags
}

resource "aws_eip_association" "fts_db_import" {
  count = local.has_import_instance ? 1 : 0

  instance_id   = aws_instance.fts_db_import[0].id
  allocation_id = aws_eip.fts_db_import[0].id

  depends_on = [aws_route53_record.fts_db_import]
}

resource "aws_route53_record" "fts_db_import" {
  count = local.has_import_instance ? 1 : 0

  zone_id = var.public_hosted_zone_id
  name    = "fts-import"
  type    = "A"
  ttl     = 60
  records = [aws_eip.fts_db_import[0].public_ip]
}
