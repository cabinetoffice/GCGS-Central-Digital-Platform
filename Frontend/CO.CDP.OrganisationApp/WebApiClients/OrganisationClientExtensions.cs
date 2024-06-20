using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using DevolvedRegulation = CO.CDP.OrganisationApp.Constants.DevolvedRegulation;

namespace CO.CDP.OrganisationApp.WebApiClients;

internal static class OrganisationClientExtensions
{
    internal static async Task<ComposedOrganisation> GetComposedOrganisation(this IOrganisationClient organisationClient, Guid organisationId)
    {
        var getOrganisationTask = organisationClient.GetOrganisationAsync(organisationId);
        var getSupplierInfoTask = organisationClient.GetOrganisationSupplierInformationAsync(organisationId);

        await Task.WhenAll(getOrganisationTask, getSupplierInfoTask);

        return new ComposedOrganisation
        {
            Organisation = getOrganisationTask.Result,
            SupplierInfo = getSupplierInfoTask.Result
        };
    }

    internal static Task UpdateBuyerOrganisationType(this IOrganisationClient organisationClient,
        Guid organisationId,
        string buyerOrganisationType)
            => organisationClient.UpdateBuyerInformationAsync(organisationId, new UpdateBuyerInformation(
                    type: BuyerInformationUpdateType.BuyerOrganisationType,
                    buyerInformation: new BuyerInformation(
                        buyerType: buyerOrganisationType,
                        devolvedRegulations: [])));

    internal static Task UpdateBuyerDevolvedRegulations(this IOrganisationClient organisationClient,
        Guid organisationId,
        List<DevolvedRegulation> devolvedRegulations)
            => organisationClient.UpdateBuyerInformationAsync(organisationId, new UpdateBuyerInformation(
                type: BuyerInformationUpdateType.DevolvedRegulation,
                buyerInformation: new BuyerInformation(
                    buyerType: null,
                    devolvedRegulations: devolvedRegulations.AsApiClientDevolvedRegulationList())));

    internal static Task UpdateOrganisationAdditionalIdentifiers(this IOrganisationClient organisationClient,
        Guid organisationId,
        ICollection<OrganisationIdentifier> additionalIdentifiers)
            => organisationClient.UpdateOrganisationAsync(organisationId,
                    new UpdatedOrganisation(
                        type: OrganisationUpdateType.AdditionalIdentifiers,
                        organisation: new OrganisationInfo(additionalIdentifiers: additionalIdentifiers, contactPoint: null, addresses: null)));

    internal static Task UpdateOrganisationContactPoint(this IOrganisationClient organisationClient,
        Guid organisationId,
        OrganisationContactPoint contactPoint)
            => organisationClient.UpdateOrganisationAsync(organisationId,
                    new UpdatedOrganisation(
                        type: OrganisationUpdateType.ContactPoint,
                        organisation: new OrganisationInfo(additionalIdentifiers: null, contactPoint: contactPoint, addresses: null)));

    internal static Task UpdateOrganisationAddresses(this IOrganisationClient organisationClient,
        Guid organisationId,
        ICollection<OrganisationAddress> addresses)
            => organisationClient.UpdateOrganisationAsync(organisationId,
                    new UpdatedOrganisation(
                        type: OrganisationUpdateType.Address,
                        organisation: new OrganisationInfo(additionalIdentifiers: null, contactPoint: null, addresses: addresses)));

    internal static Task UpdateSupplierCompletedEmailAddress(this IOrganisationClient organisationClient, Guid organisationId)
        => organisationClient.UpdateSupplierInformationAsync(
            organisationId,
            new UpdateSupplierInformation(
                type: SupplierInformationUpdateType.CompletedEmailAddress,
                supplierInformation: new SupplierInfo(supplierType: null, tradeAssurance: null, legalForm: null, qualification: null)));

    internal static Task UpdateSupplierType(this IOrganisationClient organisationClient, Guid organisationId, SupplierType supplierType)
        => organisationClient.UpdateSupplierInformationAsync(
            organisationId,
            new UpdateSupplierInformation(
                type: SupplierInformationUpdateType.SupplierType,
                supplierInformation: new SupplierInfo(supplierType: supplierType, tradeAssurance: null, legalForm: null, qualification: null)));

    internal static Task UpdateSupplierCompletedWebsiteAddress(this IOrganisationClient organisationClient, Guid organisationId)
        => organisationClient.UpdateSupplierInformationAsync(
            organisationId,
            new UpdateSupplierInformation(
                type: SupplierInformationUpdateType.CompletedWebsiteAddress,
                supplierInformation: new SupplierInfo(supplierType: null, tradeAssurance: null, legalForm: null, qualification: null)));

    internal static Task UpdateSupplierTradeAssurance(this IOrganisationClient organisationClient, Guid organisationId, TradeAssurance? tradeAssurance = null)
        => organisationClient.UpdateSupplierInformationAsync(
            organisationId,
            new UpdateSupplierInformation(
                type: SupplierInformationUpdateType.TradeAssurance,
                supplierInformation: new SupplierInfo(supplierType: null, tradeAssurance: tradeAssurance, legalForm: null, qualification: null)));
    internal static Task UpdateSupplierQualification(this IOrganisationClient organisationClient, Guid organisationId, Qualification? qualification = null)
        => organisationClient.UpdateSupplierInformationAsync(
            organisationId,
            new UpdateSupplierInformation(
                type: SupplierInformationUpdateType.Qualification,
                supplierInformation: new SupplierInfo(supplierType: null, qualification: qualification, legalForm: null, tradeAssurance: null)));
}

public class ComposedOrganisation
{
    public required Organisation.WebApiClient.Organisation Organisation { get; init; }

    public required SupplierInformation SupplierInfo { get; init; }
}