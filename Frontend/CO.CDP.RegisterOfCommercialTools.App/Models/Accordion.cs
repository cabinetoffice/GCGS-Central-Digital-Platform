namespace CO.CDP.RegisterOfCommercialTools.App.Models
{
    public class Accordion
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public Func<object, object> Content { get; set; }
    }
}