namespace CO.CDP.Forms.WebApi.Model;

public class SectionQuestionsResponse
{
    public FormSection? Section { get; init; }
    public List<FormQuestion>? Questions { get; init; }
    public List<FormAnswer>? Answers { get; init; }
}