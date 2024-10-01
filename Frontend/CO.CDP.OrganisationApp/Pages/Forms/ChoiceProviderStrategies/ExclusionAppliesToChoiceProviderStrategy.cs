namespace CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using CO.CDP.Forms.WebApiClient;
using CO.CDP.Organisation.WebApiClient;
using System.Text.Json;

public class ExclusionAppliesToChoiceProviderStrategy(IUserInfoService userInfoService, IOrganisationClient organisationClient) : IChoiceProviderStrategy
{
    public string AnswerFieldName { get; } = "JsonValue";

    public async Task<Dictionary<string, string>?> Execute(FormQuestionOptions options)
    {
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var organisationId = userInfoService.GetOrganisationId();
        if (organisationId != null)
        {
            var connectedEntities = await organisationClient.GetConnectedEntitiesAsync((Guid)organisationId);
            var organisation = await organisationClient.GetOrganisationAsync((Guid)organisationId);

            var result = new Dictionary<string, string>();

            result[JsonSerializer.Serialize(new { id = organisation.Id, type = "organisation" }, jsonSerializerOptions)] = organisation.Name;

            foreach (var entity in connectedEntities)
            {
                result[JsonSerializer.Serialize(new { id = entity.EntityId, type = "connected-entity" }, jsonSerializerOptions)] = entity.Name;
            }


            return result;
        }

        return null;
    }

    public async Task<string?> RenderOption(CO.CDP.Forms.WebApiClient.FormAnswer? answer)
    {
        // TODO: Is there a better way to accomodate both FormAnswer types without overloading?
        return await RenderOption(answer?.JsonValue);
    }

    public async Task<string?> RenderOption(CO.CDP.OrganisationApp.Models.FormAnswer? answer)
    {
        return await RenderOption(answer?.JsonValue);
    }

    private async Task<string?> RenderOption(string? jsonValue)
    {
        if (jsonValue != null)
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            ExclusionAppliesToChoiceProviderStrategyAnswer? answerValues = JsonSerializer.Deserialize<ExclusionAppliesToChoiceProviderStrategyAnswer>(jsonValue, jsonSerializerOptions);

            switch (answerValues?.Type)
            {
                case "organisation":
                    var organisation = await organisationClient.GetOrganisationAsync(answerValues.Id);
                    return organisation.Name;

                case "connected-entity":
                    var organisationId = userInfoService.GetOrganisationId();
                    if (organisationId != null)
                    {
                        var connectedEntities = await organisationClient.GetConnectedEntitiesAsync((Guid)organisationId);
                        var entity = connectedEntities.FirstOrDefault(e => e.EntityId == answerValues.Id);
                        return entity?.Name;
                    }

                    break;
            }
        }

        return null;
    }
}

public class ExclusionAppliesToChoiceProviderStrategyAnswer()
{
    public Guid Id { get; set; }
    public string Type { get; set; }
}