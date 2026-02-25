using CO.CDP.UserManagement.App.Models;

namespace CO.CDP.UserManagement.App.Services;

public interface IInviteUserStateStore
{
    Task<InviteUserState?> GetAsync();
    Task SetAsync(InviteUserState state);
    Task<InviteSuccessState?> GetSuccessAsync();
    Task SetSuccessAsync(InviteSuccessState state);
    Task ClearSuccessAsync();
    Task ClearAsync();
}
