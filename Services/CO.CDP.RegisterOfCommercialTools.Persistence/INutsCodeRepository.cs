namespace CO.CDP.RegisterOfCommercialTools.Persistence;

public interface INutsCodeRepository
{
    Task<List<NutsCode>> GetRootCodesAsync(Culture culture = Culture.English);
    Task<List<NutsCode>> GetChildrenAsync(string parentCode, Culture culture = Culture.English);
    Task<List<NutsCode>> SearchAsync(string query, Culture culture = Culture.English);
    Task<NutsCode?> GetByCodeAsync(string code);
    Task<List<NutsCode>> GetByCodesAsync(List<string> codes);
    Task<List<NutsCode>> GetHierarchyAsync(string code);
    Task<List<NutsCode>> GetAllAsync(Culture culture = Culture.English);
}