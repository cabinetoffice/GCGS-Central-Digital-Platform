namespace CO.CDP.RegisterOfCommercialTools.Persistence;

public interface ICpvCodeRepository
{
    Task<List<CpvCode>> GetRootCodesAsync(Culture culture = Culture.English);
    Task<List<CpvCode>> GetChildrenAsync(string parentCode, Culture culture = Culture.English);
    Task<List<CpvCode>> SearchAsync(string query, Culture culture = Culture.English);
    Task<CpvCode?> GetByCodeAsync(string code);
    Task<List<CpvCode>> GetByCodesAsync(List<string> codes);
    Task<List<CpvCode>> GetHierarchyAsync(string code);
    Task<List<CpvCode>> GetAllAsync(Culture culture = Culture.English);
}