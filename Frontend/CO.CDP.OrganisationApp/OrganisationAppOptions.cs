using Microsoft.AspNetCore.HttpOverrides;

namespace CO.CDP.OrganisationApp;

public class OrganisationAppOptions
{
    public const string Section = "CDP";

    public string? KnownNetwork { get; init; }
}