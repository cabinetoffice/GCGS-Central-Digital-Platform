using static CO.CDP.OrganisationApp.ThirdPartyApiClients.CompaniesHouseApi;

namespace CO.CDP.OrganisationApp.ThirdPartyApiClients;

public interface ICompaniesHouseApi
{
    Task<RegisteredAddress> GetRegisteredAddress(string companyNumber);
}
