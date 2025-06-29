resource "tls_private_key" "import_key" {
  algorithm = "RSA"
  rsa_bits  = 2048
}

resource "aws_key_pair" "import_key" {
  key_name   = "fts-db-import"
  public_key = tls_private_key.import_key.public_key_openssh
}

resource "aws_instance" "fts_db_import" {
  ami           = data.aws_ami.al2_latest.id
  instance_type = "t3.medium"
  subnet_id     = var.private_subnet_ids[0]

  key_name = aws_key_pair.import_key.key_name

  vpc_security_group_ids = [var.ec2_sg_id] # SG allowing outbound to DB on 3306

  tags = merge(
    var.tags,
    {
      Name = "fts-db-import"
    }
  )
}

output "import_instance_private_key_pem" {
  sensitive = true
  value     = tls_private_key.import_key.private_key_pem
}

output "import_instance_public_ip" {
  value = aws_instance.fts_db_import.public_ip
}
