namespace CO.CDP.DataSharing.WebApi.Model;

public class SharedDataFile
{
    public required string FileName { get; init; }

    public required string ContentType { get; init; }

    public required byte[] Content { get; init; }
}