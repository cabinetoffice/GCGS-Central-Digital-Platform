using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Services;

public interface IHierarchicalCodeService<T> where T : IHierarchicalCodeDto
{
    Task<List<T>> GetRootCodesAsync();
    Task<List<T>> GetChildrenAsync(string parentCode);
    Task<List<T>> SearchAsync(string query);
    Task<T?> GetByCodeAsync(string code);
    Task<List<T>> GetByCodesAsync(List<string> codes);
    Task<List<T>> GetHierarchyAsync(string code);
}