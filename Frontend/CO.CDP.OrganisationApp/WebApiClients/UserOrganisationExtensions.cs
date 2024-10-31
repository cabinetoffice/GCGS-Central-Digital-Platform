using CO.CDP.Tenant.WebApiClient;

namespace CO.CDP.OrganisationApp.WebApiClients;


public static class UserOrganisationExtensions
{    public static bool IsPendingBuyer(this UserOrganisation organisation)
    {
       return organisation.PendingRoles.Contains(PartyRole.Buyer);
    }
}