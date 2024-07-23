namespace CO.CDP.AwsServices;

public record AwsConfiguration
{
    public required string AccessKeyId { get; init; }
    public required string SecretAccessKey { get; init; }
    public string? Region { get; init; }
    public string? ServiceURL { get; init; }
    public Buckets? Buckets { get; init; }
}

public record Buckets
{
    public string? StagingBucket { get; set; }
    public string? PermanentBucket { get; set; }
}