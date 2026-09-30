moved {
  from = aws_wafv2_ip_set.this
  to   = aws_wafv2_ip_set.known_ips
}

