using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementCheckYourAnswersModel
{
    public List<IAnswerDisplayItem> DisplayItems { get; set; } = [];
    public FormSectionType? FormSectionType { get; set; }
}

public interface IAnswerDisplayItem
{
    bool IsGroup { get; }
}

public class GroupedAnswerSummary : IAnswerDisplayItem
{
    public string? GroupTitle { get; set; }
    public List<AnswerSummary> Answers { get; set; } = [];
    public string? GroupChangeLink { get; set; }
    public bool IsGroup => true;
}

public class AnswerSummary : IAnswerDisplayItem
{
    public string? Title { get; set; }
    public required string Answer { get; set; }
    public string? ChangeLink { get; set; }
    public bool IsGroup => false;
}