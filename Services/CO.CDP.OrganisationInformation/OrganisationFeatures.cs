namespace CO.CDP.OrganisationInformation;

/// <summary>
/// Keys of features that can be enabled for an individual organisation.
/// Stored in the <c>organisation_features</c> table and returned as <c>Details.Features</c>
/// by the Organisation API.
/// </summary>
/// <remarks>
/// The Organisation App does not reference this project, so it keeps its own copy of these keys in
/// <c>CO.CDP.OrganisationApp.Constants.OrganisationFeatures</c>. Keep the values in sync.
/// </remarks>
public static class OrganisationFeatures
{
    public const string AiTool = "ai-tool";
}
