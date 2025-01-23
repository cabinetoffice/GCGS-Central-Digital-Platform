namespace CO.CDP.AntiVirusScanner;

public interface IEvent;

public record ScanFile : IEvent
{
    public required string QueueFileName { get; set; }
    public required string UploadedFileName { get; set; }
    public Guid OrganisationId { get; set; }
    public required string UserEmailAddress { get; set; }
    public required string OrganisationEmailAddress { get; set; }
    public required string FullName { get; set; }
    public required string OrganisationName { get; set; } 
}
