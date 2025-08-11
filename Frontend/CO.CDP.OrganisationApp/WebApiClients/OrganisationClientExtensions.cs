using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using System.Linq;
using System.Net;
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
                        organisation: new OrganisationInfo(additionalIdentifiers: additionalIdentifiers, addresses: null, buyerInformation: null, contactPoint: null, identifierToRemove: null, organisationName: null, roles: null)));

    internal static Task SupportUpdateOrganisationAdditionalIdentifiers(this IOrganisationClient organisationClient,
        Guid organisationId,
        ICollection<OrganisationIdentifier> additionalIdentifiers)
        => organisationClient.SupportUpdateOrganisationAsync(organisationId,
        new SupportUpdateOrganisation(
            type: SupportOrganisationUpdateType.AdditionalIdentifiers,
            organisation: new SupportOrganisationInfo(additionalIdentifiers: additionalIdentifiers, reviewedById: null, comment: null, approved: null)));

    internal static Task UpdateOrganisationRemoveIdentifier(this IOrganisationClient organisationClient,
            Guid organisationId,
            OrganisationIdentifier identifierToRemove)
                => organisationClient.UpdateOrganisationAsync(organisationId,
                        new UpdatedOrganisation(
                            type: OrganisationUpdateType.RemoveIdentifier,
                            organisation: new OrganisationInfo(additionalIdentifiers: null, addresses: null, buyerInformation: null, contactPoint: null, identifierToRemove: identifierToRemove, organisationName: null, roles: null)));


    internal static Task UpdateOrganisationName(this IOrganisationClient organisationClient,
        Guid organisationId,
        string organisationName)
            => organisationClient.UpdateOrganisationAsync(organisationId,
                    new UpdatedOrganisation(
                        type: OrganisationUpdateType.OrganisationName,
                        organisation: new OrganisationInfo(additionalIdentifiers: null, contactPoint: null, addresses: null, buyerInformation: null, identifierToRemove: null, organisationName: organisationName, roles: null)));
    internal static Task UpdateOrganisationEmail(this IOrganisationClient organisationClient,
     Guid organisationId,
     OrganisationContactPoint contactPoint)
         => organisationClient.UpdateOrganisationAsync(organisationId,
                 new UpdatedOrganisation(
                     type: OrganisationUpdateType.OrganisationEmail,
                     organisation: new OrganisationInfo(additionalIdentifiers: null, contactPoint: contactPoint, addresses: null, buyerInformation: null, identifierToRemove: null, organisationName: null, roles: null)));

    internal static Task UpdateOrganisationAddAsBuyerRole(this IOrganisationClient organisationClient,
    Guid organisationId,
       SupplierToBuyerDetails supplierToBuyerDetails)
           => organisationClient.UpdateOrganisationAsync(organisationId,
                   new UpdatedOrganisation(
                       type: OrganisationUpdateType.AddAsBuyerRole,
                       organisation: new OrganisationInfo(additionalIdentifiers: null, contactPoint: null, addresses: null,
                           buyerInformation: new BuyerInformation(
                           buyerType: supplierToBuyerDetails.BuyerOrganisationType,
                           devolvedRegulations: supplierToBuyerDetails.Regulations.AsApiClientDevolvedRegulationList()), identifierToRemove: null, organisationName: null, roles: null)));


    internal static Task UpdateOrganisationContactPoint(this IOrganisationClient organisationClient,
        Guid organisationId,
        OrganisationContactPoint contactPoint)
            => organisationClient.UpdateOrganisationAsync(organisationId,
                    new UpdatedOrganisation(
                        type: OrganisationUpdateType.ContactPoint,
                        organisation: new OrganisationInfo(additionalIdentifiers: null, contactPoint: contactPoint, addresses: null, identifierToRemove: null, organisationName: null, roles: null, buyerInformation: null)));

    internal static Task UpdateOrganisationAddresses(this IOrganisationClient organisationClient,
        Guid organisationId,
        ICollection<OrganisationAddress> addresses)
            => organisationClient.UpdateOrganisationAsync(organisationId,
                    new UpdatedOrganisation(
                        type: OrganisationUpdateType.Address,
                        organisation: new OrganisationInfo(additionalIdentifiers: null, contactPoint: null, addresses: addresses, identifierToRemove: null, organisationName: null, roles: null, buyerInformation: null)));

    internal static Task AddOrganisationRoles(this IOrganisationClient organisationClient,
        Guid organisationId,
        ICollection<PartyRole> roles)
            => organisationClient.UpdateOrganisationAsync(organisationId,
                    new UpdatedOrganisation(
                        type: OrganisationUpdateType.AddRoles,
                        organisation: new OrganisationInfo(additionalIdentifiers: null, contactPoint: null, addresses: null, identifierToRemove: null, organisationName: null, roles: roles, buyerInformation: null)));

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

    internal static async Task<ICollection<JoinRequestLookUp>> GetOrganisationJoinRequests(this IOrganisationClient organisationClient,
        Guid organisationId, OrganisationJoinRequestStatus? status)
        => await organisationClient.GetOrganisationJoinRequestsAsync(organisationId, status);

    internal static async Task UpdateOrganisationJoinRequest(this IOrganisationClient organisationClient, Guid organisationId, Guid joinRequestId, UpdateJoinRequest updateJoinRequest)
        => await organisationClient.UpdateOrganisationJoinRequestAsync(organisationId, joinRequestId, updateJoinRequest);

    internal static async Task<bool> FeedbackAndContact(this IOrganisationClient organisationClient,
        ProvideFeedbackAndContact feedback)
        => await organisationClient.FeedbackAndContactAsync(feedback);

    internal static async Task<bool> ContactUs(this IOrganisationClient organisationClient,
        ContactUs contactUs)
        => await organisationClient.ContactUsAsync(contactUs);

    internal static async Task<List<OrganisationSummary>> GetChildOrganisationsAsync(
        this IOrganisationClient organisationClient,
        Guid parentOrganisationId)
    {
        try
        {
            var childOrganisations = await organisationClient.GetChildOrganisationsAsync(parentOrganisationId);
            return childOrganisations?.ToList() ?? new List<OrganisationSummary>();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return new List<OrganisationSummary>();
        }
    }

    internal static async Task<ICollection<OrganisationSearchResult>> SearchOrganisationAsync(
        this IOrganisationClient organisationClient,
        string name,
        string role,
        int limit,
        double threshold)
    {
        try
        {
            var searchResults = await organisationClient.SearchOrganisationAsync(name, role, limit, threshold);
            return searchResults ?? new List<OrganisationSearchResult>();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return new List<OrganisationSearchResult>();
        }
    }

    internal static async Task<Organisation.WebApiClient.Organisation?> LookupOrganisationAsync(
        this IOrganisationClient organisationClient,
        string? name,
        string? identifier)
    {
        try
        {
            return await organisationClient.LookupOrganisationAsync(name, identifier);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    internal static async Task<(ICollection<OrganisationSearchByPponResult>,int)> SearchOrganisationByNameOrPpon(
        this IOrganisationClient organisationClient,
        string searchText,
        int pageSize,
        int skip,
        string orderBy,
        double threshold) {
        try
        {
            var searchResults =  await organisationClient.SearchByNameOrPponAsync(searchText,pageSize, skip, orderBy, threshold);
            if (searchResults.Results.Count > 0)
            {
                return (searchResults.Results, searchResults.TotalCount);
            }
            return (new List<OrganisationSearchByPponResult>(), 0);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return (new List<OrganisationSearchByPponResult>(),0);
        }
    }
}

public class ComposedOrganisation
{
    public required Organisation.WebApiClient.Organisation Organisation { get; init; }

    public required SupplierInformation SupplierInfo { get; init; }
}