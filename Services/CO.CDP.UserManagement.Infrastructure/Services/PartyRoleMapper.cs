using UmPartyRole = CO.CDP.UserManagement.Core.Constants.PartyRole;

namespace CO.CDP.UserManagement.Infrastructure.Services;

public static class PartyRoleMapper
{
    public static IReadOnlyCollection<UmPartyRole> MapFromStrings(IEnumerable<string> roles) =>
        roles.Select(MapOne).ToHashSet();

    private static UmPartyRole MapOne(string role) =>
        role.ToLowerInvariant() switch
        {
            "buyer" => UmPartyRole.Buyer,
            "procuringentity" => UmPartyRole.ProcuringEntity,
            "supplier" => UmPartyRole.Supplier,
            "tenderer" => UmPartyRole.Tenderer,
            "funder" => UmPartyRole.Funder,
            "enquirer" => UmPartyRole.Enquirer,
            "payer" => UmPartyRole.Payer,
            "payee" => UmPartyRole.Payee,
            "reviewbody" => UmPartyRole.ReviewBody,
            "interestedparty" => UmPartyRole.InterestedParty,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, $"Unknown party role: {role}")
        };
}