using CO.CDP.Forms.WebApiClient;
namespace CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;

public class DefaultChoiceProviderStrategy() : IChoiceProviderStrategy
{
    public string AnswerFieldName { get; } = "OptionValue";
    public async Task<Dictionary<string, string>?> Execute(FormQuestionOptions options)
    {
        return await Task.FromResult(options.Choices.ToDictionary(c => c.Title, c => c.Title));
    }

    public async Task<string?> RenderOption(CO.CDP.Forms.WebApiClient.FormAnswer? answer, CO.CDP.Forms.WebApiClient.FormQuestion question)
    {
        if (answer?.OptionValue == null) return "";

        var choices = question.Options.Groups.SelectMany(g => g.Choices);

        var choiceOption = choices.FirstOrDefault(c => c.Value == answer.OptionValue);

        return await Task.FromResult(choiceOption == null ? answer.OptionValue : choiceOption.Title);
    }

    public async Task<string?> RenderOption(CO.CDP.OrganisationApp.Models.FormAnswer? answer, CO.CDP.OrganisationApp.Models.FormQuestion question)
    {
        if (answer?.OptionValue == null) return "";

        var choices = question.Options.Groups.SelectMany(g => g.Choices);

        var choiceOption = choices.FirstOrDefault(c => c.Value == answer.OptionValue);

        return await Task.FromResult(choiceOption == null ? answer.OptionValue : choiceOption.Title);
    }
}