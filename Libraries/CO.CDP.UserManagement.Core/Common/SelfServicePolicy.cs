namespace CO.CDP.UserManagement.Core.Common;

public static class SelfServicePolicy
{
    public static bool IsSelf(string? currentUserEmail, string? targetEmail) =>
        !string.IsNullOrEmpty(currentUserEmail) &&
        !string.IsNullOrEmpty(targetEmail) &&
        string.Equals(currentUserEmail, targetEmail, StringComparison.OrdinalIgnoreCase);
}