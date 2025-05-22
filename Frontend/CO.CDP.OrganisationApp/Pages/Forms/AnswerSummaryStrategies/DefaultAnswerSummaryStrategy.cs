using CO.CDP.Forms.WebApiClient;
using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Extensions;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;

namespace CO.CDP.OrganisationApp.Pages.Forms.AnswerSummaryStrategies;

public class DefaultAnswerSummaryStrategy : IAnswerSummaryStrategy
{
    private readonly IChoiceProviderService _choiceProviderService;

    public DefaultAnswerSummaryStrategy(IChoiceProviderService choiceProviderService)
    {
        _choiceProviderService = choiceProviderService;
    }

    public async Task<List<AnswerSummary>> GetAnswerSummaries(SectionQuestionsResponse form, FormAnswerSet answerSet)
    {
        var answerSummaries = new List<AnswerSummary>();

        foreach (FormAnswer answer in answerSet.Answers)
        {
            var question = form.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question != null && question.Type != FormQuestionType.NoInput && question.Type != FormQuestionType.CheckYourAnswers)
            {
                answerSummaries.Add(new AnswerSummary
                {
                    Title = question.SummaryTitle ?? question.Title,
                    Answer = await GetAnswerString(answer, question)
                });
            }
        }

        return answerSummaries;
    }

    private async Task<string> GetAnswerString(FormAnswer answer, FormQuestion question)
    {
        async Task<string> singleChoiceString(FormAnswer a)
        {
            var choiceProviderStrategy = _choiceProviderService.GetStrategy(question.Options.ChoiceProviderStrategy);
            return await choiceProviderStrategy.RenderOption(a) ?? "";
        }

        string boolAnswerString = answer.BoolValue.HasValue == true ? answer.BoolValue == true ? StaticTextResource.Global_Yes : StaticTextResource.Global_No : "";

        string answerString = question.Type switch
        {
            FormQuestionType.Text => answer.TextValue ?? "",
            FormQuestionType.FileUpload => answer.TextValue ?? "",
            FormQuestionType.SingleChoice => await singleChoiceString(answer),
            FormQuestionType.Date => answer.DateValue.HasValue ? answer.DateValue.Value.ToFormattedString() : "",
            FormQuestionType.CheckBox => answer.BoolValue.HasValue ? question.Options.Choices?.FirstOrDefault()?.Title ?? "" : "",
            FormQuestionType.Address => answer.AddressValue != null ? $"{answer.AddressValue.StreetAddress}, {answer.AddressValue.Locality}, {answer.AddressValue.PostalCode}, {answer.AddressValue.CountryName}" : "",
            FormQuestionType.MultiLine => answer.TextValue ?? "",
            FormQuestionType.GroupedSingleChoice => EvaluatorFactory.GroupedSingleChoiceAnswerString(answer, question),
            FormQuestionType.Url => answer.TextValue ?? "",
            _ => ""
        };

        string[] answers = [boolAnswerString, answerString];

        return string.Join(", ", answers.Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}