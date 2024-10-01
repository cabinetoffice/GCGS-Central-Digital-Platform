using CO.CDP.Tenant.WebApiClient;

namespace CO.CDP.OrganisationApp.WebApiClients;


public static class UserOrganisationExtensions
{
    public static bool IsTenderer(this UserOrganisation organisation)
    {
        return organisation.Roles.Contains(PartyRole.Tenderer);
    }

    public static bool IsBuyer(this UserOrganisation organisation)
    {
        return organisation.Roles.Contains(PartyRole.Buyer);
    }

    public static bool IsApproved(this UserOrganisation organisation)
    {
       return organisation.ApprovedOn.HasValue;        
    }
}