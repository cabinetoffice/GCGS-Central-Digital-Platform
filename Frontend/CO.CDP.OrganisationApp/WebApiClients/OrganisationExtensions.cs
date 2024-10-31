using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;

namespace CO.CDP.OrganisationApp.WebApiClients;

public static class OrganisationExtensions
{
    public static bool IsTenderer(this Organisation.WebApiClient.Organisation organisation)
    {
        return organisation.Roles.Contains(PartyRole.Tenderer);
    }

    public static bool IsBuyer(this Organisation.WebApiClient.Organisation organisation)
    {
        return organisation.Roles.Contains(PartyRole.Buyer);
    }

    public static bool IsPendingBuyer(this Organisation.WebApiClient.Organisation organisation)
    {
        return organisation.Details.PendingRoles.Contains(PartyRole.Buyer);
    }

    public static bool IsRejectedBuyer(this Organisation.WebApiClient.Organisation organisation)
    {
        return IsPendingBuyer(organisation) && organisation.Details.Approval.ReviewedBy != null;
    }

    public static ICollection<Identifier> AdditionalIdentifiersToShow(this Organisation.WebApiClient.Organisation organisationDetails)
    {              
        return organisationDetails.AdditionalIdentifiers
            .Where(identifier => identifier.Scheme != OrganisationSchemeType.VAT && identifier.Scheme != OrganisationSchemeType.Ppon && identifier.Scheme != OrganisationSchemeType.Other)
            .ToList();
    }
}