namespace CO.CDP.UserManagement.App.Models;

public abstract record ApplicationRemovalSubmitResult
{
    public sealed record NotFound : ApplicationRemovalSubmitResult;
    public sealed record Cancelled : ApplicationRemovalSubmitResult;
    public sealed record Removed : ApplicationRemovalSubmitResult;
}
