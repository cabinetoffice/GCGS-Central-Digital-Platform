using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Extensions;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using Microsoft.Extensions.Localization;

namespace CO.CDP.OrganisationApp;

public class AnswerDisplayService(
    IStringLocalizer<StaticTextResource> localizer,
    IChoiceProviderService choiceProviderService) : IAnswerDisplayService
{
    public async Task<string> FormatAnswerForDisplayAsync(QuestionAnswer questionAnswer, FormQuestion question)
    {
        var answer = questionAnswer.Answer;
        if (answer == null) return "";

        var boolAnswerString = FormatBoolValue(answer.BoolValue);
        var valueAnswerString = await FormatAnswerByTypeAsync(answer, question);

        return new[] { boolAnswerString, valueAnswerString }
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Aggregate("", (acc, s) => string.IsNullOrEmpty(acc) ? s : $"{acc}, {s}");
    }

    private string FormatBoolValue(bool? boolValue) =>
        boolValue?.ToString() switch
        {
            "True" => localizer[nameof(StaticTextResource.Global_Yes)],
            "False" => localizer[nameof(StaticTextResource.Global_No)],
            _ => ""
        };

    private async Task<string> FormatAnswerByTypeAsync(FormAnswer answer, FormQuestion question) =>
        question.Type switch
        {
            FormQuestionType.Text or FormQuestionType.FileUpload or FormQuestionType.MultiLine or FormQuestionType.Url
                => answer.TextValue ?? "",
            FormQuestionType.SingleChoice => await GetSingleChoiceAnswerStringAsync(answer, question),
            FormQuestionType.Date => answer.DateValue?.ToFormattedString() ?? "",
            FormQuestionType.CheckBox => answer.BoolValue == true
                ? question.Options.Choices?.Values.FirstOrDefault() ?? ""
                : "",
            FormQuestionType.Address => answer.AddressValue?.ToHtmlString() ?? "",
            FormQuestionType.GroupedSingleChoice => GetGroupedSingleChoiceAnswerString(answer, question),
            _ => ""
        };

    private async Task<string> GetSingleChoiceAnswerStringAsync(FormAnswer answer, FormQuestion question)
    {
        var choiceProviderStrategy = choiceProviderService.GetStrategy(question.Options.ChoiceProviderStrategy);
        return await choiceProviderStrategy.RenderOption(answer) ?? "";
    }

    private static string GetGroupedSingleChoiceAnswerString(FormAnswer? answer, FormQuestion question)
    {
        return question.Options.Groups
            .SelectMany(g => g.Choices)
            .Where(c => c.Value == answer?.OptionValue)
            .Select(c => c.Title)
            .FirstOrDefault() ?? answer?.OptionValue ?? "";
    }

    public AnswerSummary CreateAnswerSummary(
        FormQuestion question, QuestionAnswer questionAnswer, string answerString,
        Guid organisationId, Guid formId, Guid sectionId)
    {
        var changeLink =
            $"/organisation/{organisationId}/forms/{formId}/sections/{sectionId}/questions/{question.Id}?frm-chk-answer=true";

        var isNonUkAddress = question.Type == FormQuestionType.Address
                             && questionAnswer.Answer?.AddressValue?.Country != Country.UKCountryCode;

        if (isNonUkAddress)
        {
            changeLink += "&UkOrNonUk=non-uk";
        }

        return new AnswerSummary
        {
            Title = question.SummaryTitle ?? question.Title,
            Answer = answerString,
            ChangeLink = changeLink
        };
    }

    public async Task<AnswerSummary?> CreateIndividualAnswerSummaryAsync(
        FormQuestion question, FormQuestionAnswerState answerState, Guid organisationId, Guid formId, Guid sectionId)
    {
        var questionAnswer = answerState.Answers
            .FirstOrDefault(a => a.QuestionId == question.Id && a.Answer != null);

        if (questionAnswer == null) return null;

        var answerString = await FormatAnswerForDisplayAsync(questionAnswer, question);
        return string.IsNullOrWhiteSpace(answerString)
            ? null
            : CreateAnswerSummary(question, questionAnswer, answerString, organisationId, formId, sectionId);
    }

    public async Task<GroupedAnswerSummary> CreateMultiQuestionGroupAsync(
        FormQuestion startingQuestion, List<FormQuestion> orderedJourney, FormQuestionAnswerState answerState,
        Guid organisationId, Guid formId, Guid sectionId, FormQuestionGrouping grouping,
        Func<List<FormQuestion>, FormQuestion?> getFirstQuestion)
    {
        var questionsInGroup = orderedJourney
            .Where(q => q.Options.Grouping?.Id == grouping.Id)
            .ToList();

        var answerTasks = questionsInGroup
            .Select(async q =>
                await CreateIndividualAnswerSummaryAsync(q, answerState, organisationId, formId, sectionId));

        var answers = (await Task.WhenAll(answerTasks))
            .OfType<AnswerSummary>()
            .ToList();

        var firstQuestionInGroup = getFirstQuestion(questionsInGroup) ?? startingQuestion;

        var groupChangeLink = grouping.Page
            ? $"/organisation/{organisationId}/forms/{formId}/sections/{sectionId}/questions/{firstQuestionInGroup.Id}"
            : null;

        if (!grouping.Page)
        {
            foreach (var answer in answers)
            {
                var matchingQuestion = questionsInGroup.First(q =>
                    q.Title == answer.Title || (q.SummaryTitle != null && q.SummaryTitle == answer.Title));
                answer.ChangeLink =
                    $"/organisation/{organisationId}/forms/{formId}/sections/{sectionId}/questions/{matchingQuestion.Id}?frm-chk-answer=true";
            }
        }

        return new GroupedAnswerSummary
        {
            GroupTitle = !string.IsNullOrEmpty(grouping.SummaryTitle)
                ? grouping.SummaryTitle
                : startingQuestion.SummaryTitle,
            GroupChangeLink = groupChangeLink,
            Answers = answers
        };
    }
}