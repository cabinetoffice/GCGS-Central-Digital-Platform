using CO.CDP.Forms.WebApiClient;
namespace CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;

public class DefaultChoiceProviderStrategy() : IChoiceProviderStrategy
{
    public string AnswerFieldName { get; } = "OptionValue";
    public async Task<Dictionary<string, string>?> Execute(FormQuestionOptions options)
    {
        return await Task.FromResult(options.Choices.ToDictionary(c => c.Title, c => c.Title));
    }

    public async Task<string?> RenderOption(CO.CDP.Forms.WebApiClient.FormAnswer? answer)
    {
        // TODO: Is there a better way to accomodate both FormAnswer types without overloading?
        return await RenderOption(answer?.OptionValue);
    }

    public async Task<string?> RenderOption(CO.CDP.OrganisationApp.Models.FormAnswer? answer)
    {
        return await RenderOption(answer?.OptionValue);
    }

    private async Task<string?> RenderOption(string? optionValue)
    {
        return optionValue;
    }
}