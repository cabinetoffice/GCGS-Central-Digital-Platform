namespace CO.CDP.OrganisationApp.ThirdPartyApiClients.CompaniesHouse;

public interface ICompaniesHouseApi
{
    Task<RegisteredAddress?> GetRegisteredAddress(string companyNumber);
    Task<CompanyProfile?> GetProfile(string companyNumber);
}
