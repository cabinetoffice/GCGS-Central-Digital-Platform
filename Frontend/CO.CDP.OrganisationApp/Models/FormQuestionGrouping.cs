namespace CO.CDP.OrganisationApp.Models;

public class FormQuestionGrouping
{
    public PageGrouping? Page { get; set; }
    public CheckYourAnswersGrouping? CheckYourAnswers { get; set; }
}

public class PageGrouping
{
    public int NextQuestionsToDisplay { get; set; }
    public string? PageTitleResourceKey { get; set; }
    public string? SubmitButtonTextResourceKey { get; set; }
}

public class CheckYourAnswersGrouping
{
    public string? GroupTitleResourceKey { get; set; }
}
