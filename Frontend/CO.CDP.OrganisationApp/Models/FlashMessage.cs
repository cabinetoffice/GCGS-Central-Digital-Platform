namespace CO.CDP.OrganisationApp.Models;

public class FlashMessage
{
    public FlashMessage(string heading, string? description = null)
    {
        Heading = heading;

        if(!string.IsNullOrEmpty(description))
        {
            Description = description;
        }
    }

    public string Heading { get; set; }
    public string? Description { get; set; }
}
