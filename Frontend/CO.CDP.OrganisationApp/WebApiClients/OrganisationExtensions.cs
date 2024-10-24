using CO.CDP.Organisation.WebApiClient;

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
}