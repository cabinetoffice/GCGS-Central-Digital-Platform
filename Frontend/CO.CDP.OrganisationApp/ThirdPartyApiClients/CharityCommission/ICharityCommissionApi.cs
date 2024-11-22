using CO.CDP.OrganisationApp.CharityCommission;

namespace CO.CDP.OrganisationApp.ThirdPartyApiClients.CharityCommission;

public interface ICharityCommissionApi
{
    Task<CharityDetails?> GetCharityDetails(string registeredNumber);
}
