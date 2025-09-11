using CO.CDP.RegisterOfCommercialTools.App.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Services;

public interface ICpvCodeService
{
    Task<List<CpvCode>> GetRootCpvCodesAsync();
    Task<List<CpvCode>> GetChildrenAsync(string parentCode);
    Task<List<CpvCode>> SearchAsync(string query);
    Task<CpvCode?> GetByCodeAsync(string code);
    Task<List<CpvCode>> GetByCodesAsync(List<string> codes);
    Task<List<CpvCode>> GetHierarchyAsync(string code);
}