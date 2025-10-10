namespace CO.CDP.UI.Foundation.Models;

public class FlashMessage
{
    public FlashMessage(string heading, string? description = null, string? title = null)
    {
        Heading = heading;

        if (!string.IsNullOrEmpty(description))
        {
            Description = description;
        }

        Title = title;
    }

    public string? Title { get; set; }
    public string Heading { get; set; }
    public string? Description { get; set; }
}