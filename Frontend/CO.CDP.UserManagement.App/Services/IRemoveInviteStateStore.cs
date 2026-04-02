using CO.CDP.UserManagement.App.Models;

namespace CO.CDP.UserManagement.App.Services;

public interface IRemoveInviteStateStore
{
    Task<RemoveInviteSuccessState?> GetAsync();
    Task SetAsync(RemoveInviteSuccessState state);
    Task ClearAsync();
}
