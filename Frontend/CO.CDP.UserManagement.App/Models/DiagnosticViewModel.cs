namespace CO.CDP.UserManagement.App.Models;

public sealed record DiagnosticViewModel(
    IReadOnlyList<DiagnosticServiceViewModel> Services);

public sealed record DiagnosticServiceViewModel(
    string Name,
    string? Url)
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(Url);
}
