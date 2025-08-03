using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Extensions;
using CO.CDP.OrganisationApp.Models;
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
}