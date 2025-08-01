using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementCheckYourAnswersModel
{
    public List<IAnswerDisplayItem> DisplayItems { get; set; } = [];
    public FormSectionType? FormSectionType { get; set; }

    public IEnumerable<IAnswerDisplayItem> GroupedDisplayItems => 
        DisplayItems.GroupConsecutiveSingleQuestions();
}

public static class DisplayItemExtensions
{
    public static IEnumerable<IAnswerDisplayItem> GroupConsecutiveSingleQuestions(this IEnumerable<IAnswerDisplayItem> items)
    {
        return items
            .Aggregate(
                new List<List<IAnswerDisplayItem>>(),
                (groups, item) => item.IsGroup 
                    ? groups.Append([item]).ToList()
                    : groups.Any() && !groups.Last().First().IsGroup
                        ? groups.Take(groups.Count - 1).Append(groups.Last().Append(item).ToList()).ToList()
                        : groups.Append([item]).ToList())
            .Select(group => group.First().IsGroup 
                ? group.Single()
                : new SingleQuestionsGroup { Answers = group.Cast<AnswerSummary>().ToList() });
    }
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

public class SingleQuestionsGroup : IAnswerDisplayItem
{
    public List<AnswerSummary> Answers { get; set; } = [];
    public bool IsGroup => true;
}