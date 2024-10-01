namespace CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using CO.CDP.Forms.WebApiClient;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.Tenant.WebApiClient;

public class ExclusionAppliesToChoiceProviderStrategy(IUserInfoService userInfoService, IOrganisationClient organisationClient) : IChoiceProviderStrategy
{
    public async Task<List<string>?> Execute(FormQuestionOptions options)
    {
        var organisationId = userInfoService.GetOrganisationId();
        if (organisationId != null)
        {
            var connectedEntities = await organisationClient.GetConnectedEntitiesAsync((Guid)organisationId);
            List<string> returnList = [
                (await organisationClient.GetOrganisationAsync((Guid)organisationId)).Name
            ];
            returnList.AddRange(connectedEntities.Select(entity => entity.Name));

            return returnList;
        }

        return null;
    }
}
