using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Extensions;

/// <summary>
/// Extension methods for mapping PartyRole to localised display text.
/// </summary>
public static class PartyRoleDisplayExtensions
{
    /// <summary>
    /// Returns the localised display text for a party role.
    /// </summary>
    /// <param name="role">The party role.</param>
    /// <returns>The localised display text for the role.</returns>
    public static string GetDisplayText(this PartyRole role)
    {
        return role switch
        {
            PartyRole.Buyer => StaticTextResource.Global_Buyer,
            PartyRole.Supplier => StaticTextResource.Global_Supplier,
            PartyRole.ProcuringEntity => StaticTextResource.Global_ProcuringEntity,
            PartyRole.Tenderer => StaticTextResource.Global_Tenderer,
            PartyRole.Funder => StaticTextResource.Global_Funder,
            PartyRole.Enquirer => StaticTextResource.Global_Enquirer,
            PartyRole.Payer => StaticTextResource.Global_Payer,
            PartyRole.Payee => StaticTextResource.Global_Payee,
            PartyRole.ReviewBody => StaticTextResource.Global_ReviewBody,
            PartyRole.InterestedParty => StaticTextResource.Global_InterestedParty,
            _ => StaticTextResource.Global_Unknown
        };
    }

    /// <summary>
    /// Returns a comma-separated list of localised display texts for multiple party roles.
    /// </summary>
    /// <param name="roles">The collection of party roles.</param>
    /// <returns>A comma-separated list of localised display texts.</returns>
    public static string GetDisplayText(this IEnumerable<PartyRole> roles)
    {
        var roleTexts = roles?.Select(r => r.GetDisplayText()).Distinct() ?? [];
        return string.Join(", ", roleTexts);
    }
}
