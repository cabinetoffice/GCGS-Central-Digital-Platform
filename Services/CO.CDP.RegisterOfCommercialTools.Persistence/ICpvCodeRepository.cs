namespace CO.CDP.RegisterOfCommercialTools.Persistence;

public interface ICpvCodeRepository
{
    Task<List<CpvCode>> GetRootCodesAsync();
    Task<List<CpvCode>> GetChildrenAsync(string parentCode);
    Task<List<CpvCode>> SearchAsync(string query);
    Task<CpvCode?> GetByCodeAsync(string code);
    Task<List<CpvCode>> GetByCodesAsync(List<string> codes);
    Task<List<CpvCode>> GetHierarchyAsync(string code);
    Task<List<CpvCode>> GetAllAsync();
}