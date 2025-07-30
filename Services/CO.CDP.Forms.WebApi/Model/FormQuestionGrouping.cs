namespace CO.CDP.Forms.WebApi.Model;

public class FormQuestionGrouping
{
    public Guid Id { get; set; }
    public bool CheckYourAnswers { get; set; }
    public bool Page { get; set; }
    public string? SummaryTitle { get; set; }
}
