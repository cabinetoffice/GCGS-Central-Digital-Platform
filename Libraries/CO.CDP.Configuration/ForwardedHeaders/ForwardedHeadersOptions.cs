namespace CO.CDP.Configuration.ForwardedHeaders;

public class ForwardedHeadersOptions
{
    public const string Section = "ForwardedHeaders";

    public string? KnownNetwork { get; init; }
}