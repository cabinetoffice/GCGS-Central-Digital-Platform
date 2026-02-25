using CO.CDP.UserManagement.App.Models;

namespace CO.CDP.UserManagement.App.Services;

public interface IChangeRoleStateStore
{
    Task<ChangeRoleState?> GetAsync();
    Task SetAsync(ChangeRoleState state);
    Task ClearAsync();
}
