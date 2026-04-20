namespace CO.CDP.UserManagement.App.Models;

public abstract record UserRemovalSubmitResult
{
    public sealed record NotFound : UserRemovalSubmitResult;
    public sealed record ValidationError(string Message) : UserRemovalSubmitResult;
    public sealed record Cancelled : UserRemovalSubmitResult;
    public sealed record Removed : UserRemovalSubmitResult;
}
