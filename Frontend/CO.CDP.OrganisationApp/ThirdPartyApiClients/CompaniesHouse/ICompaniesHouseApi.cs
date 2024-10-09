using System.Net;

namespace CO.CDP.OrganisationApp.ThirdPartyApiClients.CompaniesHouse;

public interface ICompaniesHouseApi
{
    Task<(RegisteredAddress?, HttpStatusCode)> GetRegisteredAddress(string companyNumber);
    Task<(CompanyProfile?, HttpStatusCode)> GetProfile(string companyNumber);
}
