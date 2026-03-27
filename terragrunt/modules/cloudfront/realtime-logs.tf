resource "aws_kinesis_stream" "realtime_logs" {
  count = var.cloudfront_enabled && var.cloudfront_realtime_logs_enabled ? 1 : 0

  name        = local.realtime_log_stream_name
  shard_count = 1

  retention_period = 24
  tags             = var.tags
}

resource "aws_cloudfront_realtime_log_config" "this" {
  count = var.cloudfront_enabled && var.cloudfront_realtime_logs_enabled ? 1 : 0

  name          = local.realtime_log_base_name
  sampling_rate = var.cloudfront_realtime_logs_sampling_rate
  fields        = var.cloudfront_realtime_logs_fields

  endpoint {
    stream_type = "Kinesis"

    kinesis_stream_config {
      role_arn   = var.cloudfront_realtime_logs_role_arn
      stream_arn = aws_kinesis_stream.realtime_logs[0].arn
    }
  }
}
