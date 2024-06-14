using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using DevolvedRegulation = CO.CDP.OrganisationApp.Constants.DevolvedRegulation;

namespace CO.CDP.OrganisationApp.WebApiClients;

internal static class OrganisationClientExtensions
{
    internal static Task UpdateBuyerOrganisationType(this IOrganisationClient organisationClient,
        Guid organisationId,
        string buyerOrganisationType
    ) => organisationClient.UpdateBuyerInformationAsync(organisationId, new UpdateBuyerInformation(
        type: BuyerInformationUpdateType.BuyerOrganisationType,
        buyerInformation: new BuyerInformation(
            buyerType: buyerOrganisationType,
            devolvedRegulations: [])));

    internal static Task UpdateBuyerDevolvedRegulations(this IOrganisationClient organisationClient,
        Guid organisationId,
        List<DevolvedRegulation> devolvedRegulations
    ) => organisationClient.UpdateBuyerInformationAsync(organisationId, new UpdateBuyerInformation(
        type: BuyerInformationUpdateType.DevolvedRegulation,
        buyerInformation: new BuyerInformation(
            buyerType: null,
            devolvedRegulations: devolvedRegulations.AsApiClientDevolvedRegulationList())));
}