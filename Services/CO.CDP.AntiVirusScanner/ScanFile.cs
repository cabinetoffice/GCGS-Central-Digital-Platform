namespace CO.CDP.AntiVirusScanner;

public interface IEvent;

public record ScanFile : IEvent
{
    public string FileName { get; set; }
    public Guid OrganisationId { get; set; }
}
