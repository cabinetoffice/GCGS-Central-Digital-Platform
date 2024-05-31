namespace CompanyHouseApi.Integration.ExternalServices.CompaniesHouse;

public interface ICompaniesHouseService
{
    Task<CompanyHouseDetails> GetCompanyAsync(string companyNumber);
    Task<List<Officer>> GetCompanyOfficersListAsync(string companyNumber);
}
