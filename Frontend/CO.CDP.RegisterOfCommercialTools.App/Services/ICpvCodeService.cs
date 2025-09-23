using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Services;

public interface ICpvCodeService
{
    Task<List<CpvCodeDto>> GetRootCpvCodesAsync();
    Task<List<CpvCodeDto>> GetChildrenAsync(string parentCode);
    Task<List<CpvCodeDto>> SearchAsync(string query);
    Task<CpvCodeDto?> GetByCodeAsync(string code);
    Task<List<CpvCodeDto>> GetByCodesAsync(List<string> codes);
    Task<List<CpvCodeDto>> GetHierarchyAsync(string code);
}