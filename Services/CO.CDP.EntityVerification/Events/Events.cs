namespace CO.CDP.EntityVerification.Events;

public interface IEvent;

public record OrganisationRegistered : IEvent
{
    public required string Name { get; set;}
}
