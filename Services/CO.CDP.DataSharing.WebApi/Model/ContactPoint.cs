namespace CO.CDP.DataSharing.WebApi.Model;

internal record ContactPoint
{
    public string? Email { get; init; }
    public string? Telephone { get; init; }
    public string? FaxNumber { get; init; }
}