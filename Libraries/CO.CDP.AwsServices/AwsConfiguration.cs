namespace CO.CDP.AwsServices;

public record AwsConfiguration
{
    public Credentials? Credentials { get; init; }
    public string? ServiceURL { get; init; }
    public Buckets? Buckets { get; init; }
    public SqsDispatcherConfiguration? SqsDispatcher { get; init; }
    public SqsPublisherConfiguration? SqsPublisher { get; init; }
}

public record Credentials
{
    public required string AccessKeyId { get; init; }
    public required string SecretAccessKey { get; init; }
}

public record Buckets
{
    public string? StagingBucket { get; set; }
    public string? PermanentBucket { get; set; }
}

public record SqsDispatcherConfiguration
{
    public required string QueueUrl { get; init; }
    public required int MaxNumberOfMessages { get; init; } = 1;
    public required int WaitTimeSeconds { get; init; } = 30;
}

public record SqsPublisherConfiguration
{
    public required string QueueUrl { get; init; }
    public required string? MessageGroupId { get; init; }
}