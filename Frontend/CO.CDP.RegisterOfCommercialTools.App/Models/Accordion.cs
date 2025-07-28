namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public class Accordion
{
    public string Id { get; }
    public string Title { get; }
    public string PartialViewName { get; }
    public object PartialViewModel { get; }
    public bool IsOpen { get; set; }

    public Accordion(string id, string title, string partialViewName, object partialViewModel)
    {
        Id = id;
        Title = title;
        PartialViewName = partialViewName;
        PartialViewModel = partialViewModel;
    }
}