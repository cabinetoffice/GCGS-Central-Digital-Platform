namespace CO.CDP.RegisterOfCommercialTools.App.Models
{
    public class Accordion
    {
        public required string Id { get; set; }
        public required string Title { get; set; }
        public required Func<object, object> Content { get; set; }
    }
}