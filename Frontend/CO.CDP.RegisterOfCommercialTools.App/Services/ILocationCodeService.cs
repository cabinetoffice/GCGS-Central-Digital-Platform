using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Services;

public interface ILocationCodeService
{
    Task<List<NutsCodeDto>> GetRootLocationCodesAsync();
    Task<List<NutsCodeDto>> GetChildrenAsync(string parentCode);
    Task<List<NutsCodeDto>> SearchAsync(string query);
    Task<NutsCodeDto?> GetByCodeAsync(string code);
    Task<List<NutsCodeDto>> GetByCodesAsync(List<string> codes);
    Task<List<NutsCodeDto>> GetHierarchyAsync(string code);
}