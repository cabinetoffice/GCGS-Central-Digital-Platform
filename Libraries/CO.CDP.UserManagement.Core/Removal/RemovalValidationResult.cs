namespace CO.CDP.UserManagement.Core.Removal;

public sealed record RemovalValidationResult(
    bool IsValid,
    string? ErrorMessage = null)
{
    public static RemovalValidationResult Success() => new(true);
    public static RemovalValidationResult Fail(string message) => new(false, message);
}