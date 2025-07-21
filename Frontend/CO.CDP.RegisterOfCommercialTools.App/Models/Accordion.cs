namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public class Accordion(string id, string title, Func<object, object> content)
{
    public string Id { get; init; } = id ?? throw new ArgumentNullException(nameof(id));
    public string Title { get; init; } = title ?? throw new ArgumentNullException(nameof(title));
    public Func<object, object> Content { get; init; } = content ?? throw new ArgumentNullException(nameof(content));
    public bool IsOpen { get; set; }
}