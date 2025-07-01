resource "tls_private_key" "import_key" {
  algorithm = "RSA"
  rsa_bits  = 2048
}

resource "aws_key_pair" "import_key" {
  key_name   = "fts-db-import"
  public_key = tls_private_key.import_key.public_key_openssh
}

resource "aws_instance" "fts_db_import" {
  count = var.environment == "development" ? 1 : 0

  ami           = data.aws_ami.al2_latest[0].id
  instance_type = "t3.medium"
  subnet_id     = var.public_subnet_ids[0]

  key_name = aws_key_pair.import_key.key_name

  vpc_security_group_ids = [var.ec2_sg_id]

  tags = merge(
    var.tags,
    {
      Name = "fts-db-import"
    }
  )
}

