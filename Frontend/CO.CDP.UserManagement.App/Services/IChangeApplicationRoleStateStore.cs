using CO.CDP.UserManagement.App.Models;

namespace CO.CDP.UserManagement.App.Services;

public interface IChangeApplicationRoleStateStore
{
    Task<ChangeApplicationRoleState?> GetAsync();
    Task SetAsync(ChangeApplicationRoleState state);
    Task ClearAsync();
}
