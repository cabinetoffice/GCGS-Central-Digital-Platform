namespace CO.CDP.Forms.WebApi.Model;

public class SectionQuestionsResponse
{
    public FormSection? Section { get; init; }

    public List<FormQuestion> Questions { get; init; } = [];

    public List<FormAnswerSet> AnswerSets { get; init; } = [];
}