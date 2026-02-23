using CO.CDP.UserManagement.App.Models;

namespace CO.CDP.UserManagement.App.Services;

public interface IInviteUserStateStore
{
    Task<InviteUserState?> GetAsync();
    Task SetAsync(InviteUserState state);
    Task ClearAsync();
}
