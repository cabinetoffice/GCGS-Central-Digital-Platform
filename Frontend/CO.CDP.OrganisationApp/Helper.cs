namespace CO.CDP.OrganisationApp;

public static class Helper
{
    public static bool ValidRelativeUri(string? redirectUri)
        => !string.IsNullOrWhiteSpace(redirectUri) && Uri.TryCreate(redirectUri, UriKind.Relative, out var _);
}