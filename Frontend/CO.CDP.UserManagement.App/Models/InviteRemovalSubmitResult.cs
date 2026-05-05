namespace CO.CDP.UserManagement.App.Models;

public abstract record InviteRemovalSubmitResult
{
    public sealed record NotFound : InviteRemovalSubmitResult;
    public sealed record Removed : InviteRemovalSubmitResult;
}
