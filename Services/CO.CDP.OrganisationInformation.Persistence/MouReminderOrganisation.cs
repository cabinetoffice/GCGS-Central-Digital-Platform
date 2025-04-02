namespace CO.CDP.OrganisationInformation.Persistence;

public record MouReminderOrganisation
{
    public int Id { get; set; }

    public Guid Guid { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }
}