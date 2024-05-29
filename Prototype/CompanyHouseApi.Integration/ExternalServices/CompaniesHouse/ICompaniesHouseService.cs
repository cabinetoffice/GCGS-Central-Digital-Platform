namespace CompanyHouseApi.Integration.ExternalServices.CompaniesHouse;

public interface ICompaniesHouseService
{
    Task<CompanyHouseDetails> GetCompanyAsync(string companyNumber);
}
