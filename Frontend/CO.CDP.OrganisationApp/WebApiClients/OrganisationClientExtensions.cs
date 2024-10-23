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

    internal static async Task<ICollection<ConnectedEntityLookup>> GetConnectedEntities(this IOrganisationClient organisationClient, Guid organisationId)
    {
        return await organisationClient.GetConnectedEntitiesAsync(organisationId);
    }

    internal static async Task RegisterConnectedPerson(this IOrganisationClient organisationClient,
        Guid organisationId,
        RegisterConnectedEntity? registerConnectedEntity)
            => await organisationClient.CreateConnectedEntityAsync(organisationId, registerConnectedEntity);

    internal static async Task UpdateConnectedPerson(this IOrganisationClient organisationClient,
        Guid organisationId,
        Guid connectedPersonId,
        UpdateConnectedEntity? updatedConnectedEntity)
        => await organisationClient.UpdateConnectedEntityAsync(organisationId, connectedPersonId, updatedConnectedEntity);

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
                        organisation: new OrganisationInfo(additionalIdentifiers: additionalIdentifiers, contactPoint: null, addresses: null, identifierToRemove: null, organisationName: null, roles: null)));

    internal static Task UpdateOrganisationRemoveIdentifier(this IOrganisationClient organisationClient,
            Guid organisationId,
            OrganisationIdentifier identifierToRemove)
                => organisationClient.UpdateOrganisationAsync(organisationId,
                        new UpdatedOrganisation(
                            type: OrganisationUpdateType.RemoveIdentifier,
                            organisation: new OrganisationInfo(additionalIdentifiers: null, contactPoint: null, addresses: null, identifierToRemove: identifierToRemove, organisationName: null, roles: null)));


    internal static Task UpdateOrganisationName(this IOrganisationClient organisationClient,
        Guid organisationId,
        string organisationName)
            => organisationClient.UpdateOrganisationAsync(organisationId,
                    new UpdatedOrganisation(
                        type: OrganisationUpdateType.OrganisationName,
                        organisation: new OrganisationInfo(additionalIdentifiers: null, contactPoint: null, addresses: null, identifierToRemove: null, organisationName: organisationName, roles: null)));
    internal static Task UpdateOrganisationEmail(this IOrganisationClient organisationClient,
     Guid organisationId,
     OrganisationContactPoint contactPoint)
         => organisationClient.UpdateOrganisationAsync(organisationId,
                 new UpdatedOrganisation(
                     type: OrganisationUpdateType.OrganisationEmail,
                     organisation: new OrganisationInfo(additionalIdentifiers: null, contactPoint: contactPoint, addresses: null, identifierToRemove: null, organisationName: null, roles: null)));

    internal static Task UpdateOrganisationContactPoint(this IOrganisationClient organisationClient,
        Guid organisationId,
        OrganisationContactPoint contactPoint)
            => organisationClient.UpdateOrganisationAsync(organisationId,
                    new UpdatedOrganisation(
                        type: OrganisationUpdateType.ContactPoint,
                        organisation: new OrganisationInfo(additionalIdentifiers: null, contactPoint: contactPoint, addresses: null, identifierToRemove: null, organisationName: null, roles: null)));

    internal static Task UpdateOrganisationAddresses(this IOrganisationClient organisationClient,
        Guid organisationId,
        ICollection<OrganisationAddress> addresses)
            => organisationClient.UpdateOrganisationAsync(organisationId,
                    new UpdatedOrganisation(
                        type: OrganisationUpdateType.Address,
                        organisation: new OrganisationInfo(additionalIdentifiers: null, contactPoint: null, addresses: addresses, identifierToRemove: null, organisationName: null, roles: null)));

    internal static Task AddOrganisationRoles(this IOrganisationClient organisationClient,
        Guid organisationId,
        ICollection<PartyRole> roles)
            => organisationClient.UpdateOrganisationAsync(organisationId,
                    new UpdatedOrganisation(
                        type: OrganisationUpdateType.AddRoles,
                        organisation: new OrganisationInfo(additionalIdentifiers: null, contactPoint: null, addresses: null, identifierToRemove: null, organisationName: null, roles: roles)));

    internal static Task UpdateSupplierCompletedEmailAddress(this IOrganisationClient organisationClient, Guid organisationId)
        => organisationClient.UpdateSupplierInformationAsync(
            organisationId,
            new UpdateSupplierInformation(
                type: SupplierInformationUpdateType.CompletedEmailAddress,
                supplierInformation: new SupplierInfo(supplierType: null, operationTypes: null, legalForm: null)));

    internal static Task UpdateSupplierType(this IOrganisationClient organisationClient, Guid organisationId, SupplierType supplierType)
        => organisationClient.UpdateSupplierInformationAsync(
            organisationId,
            new UpdateSupplierInformation(
                type: SupplierInformationUpdateType.SupplierType,
                supplierInformation: new SupplierInfo(supplierType: supplierType, operationTypes: null, legalForm: null)));

    internal static Task UpdateSupplierCompletedWebsiteAddress(this IOrganisationClient organisationClient, Guid organisationId)
        => organisationClient.UpdateSupplierInformationAsync(
            organisationId,
            new UpdateSupplierInformation(
                type: SupplierInformationUpdateType.CompletedWebsiteAddress,
                supplierInformation: new SupplierInfo(supplierType: null, operationTypes: null, legalForm: null)));

    internal static Task UpdateSupplierLegalForm(this IOrganisationClient organisationClient, Guid organisationId, LegalForm? legalForm = null)
        => organisationClient.UpdateSupplierInformationAsync(
            organisationId,
            new UpdateSupplierInformation(
                type: SupplierInformationUpdateType.LegalForm,
                supplierInformation: new SupplierInfo(supplierType: null, operationTypes: null, legalForm: legalForm)));

    internal static Task UpdateOperationType(this IOrganisationClient organisationClient, Guid organisationId, List<OperationType>? operationTypes)
        => organisationClient.UpdateSupplierInformationAsync(
            organisationId,
            new UpdateSupplierInformation(
                type: SupplierInformationUpdateType.OperationType,
                supplierInformation: new SupplierInfo(supplierType: null, operationTypes: operationTypes, legalForm: null)));

    internal static Task UpdateSupplierCompletedConnectedPerson(this IOrganisationClient organisationClient, Guid organisationId)
        => organisationClient.UpdateSupplierInformationAsync(
            organisationId,
            new UpdateSupplierInformation(
                type: SupplierInformationUpdateType.CompletedConnectedPerson,
                supplierInformation: new SupplierInfo(supplierType: null, operationTypes: null, legalForm: null)));

    internal static async Task RevokeAuthenticationKey(this IOrganisationClient organisationClient,
        Guid organisationId, string authenticationKeyName)
        => await organisationClient.RevokeAuthenticationKeyAsync(organisationId, authenticationKeyName);

    internal static async Task CreateAuthenticationKey(this IOrganisationClient organisationClient,
        Guid organisationId, RegisterAuthenticationKey? registerAuthenticationKey)
        => await organisationClient.CreateAuthenticationKeyAsync(organisationId, registerAuthenticationKey);

    internal static async Task<ICollection<AuthenticationKey>> GetAuthenticationKeys(this IOrganisationClient organisationClient,
        Guid organisationId)
        => await organisationClient.GetAuthenticationKeysAsync(organisationId);

    internal static Task UpdateSupplierCompletedVat(this IOrganisationClient organisationClient, Guid organisationId)
        => organisationClient.UpdateSupplierInformationAsync(
            organisationId,
            new UpdateSupplierInformation(
                type: SupplierInformationUpdateType.CompletedVat,
                supplierInformation: new SupplierInfo(supplierType: null, operationTypes: null, legalForm: null)));

    internal static async Task<bool> FeedbackAndContact(this IOrganisationClient organisationClient,
        ProvideFeedbackAndContact feedback)
        => await organisationClient.FeedbackAndContactAsync(feedback);
}

public class ComposedOrganisation
{
    public required Organisation.WebApiClient.Organisation Organisation { get; init; }

    public required SupplierInformation SupplierInfo { get; init; }
}