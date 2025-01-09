namespace CO.CDP.AntiVirusScanner;

public interface IEvent;

public record ScanFile : IEvent
{
    public string FileName { get; set; } = string.Empty;
    public Guid OrganisationId { get; set; }
}
