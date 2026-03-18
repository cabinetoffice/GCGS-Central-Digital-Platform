using CO.CDP.Organisation.WebApiClient;
using CO.CDP.UserManagement.Core.Interfaces;
using OiPartyRole = CO.CDP.Organisation.WebApiClient.PartyRole;
using CorePartyRole = CO.CDP.UserManagement.Core.Constants.PartyRole;

namespace CO.CDP.UserManagement.CdpInfrastructure;

public class OrganisationApiAdapter(IOrganisationClient organisationClient) : IOrganisationApiAdapter
{
    public async Task<ISet<CorePartyRole>> GetPartyRolesAsync(
        Guid cdpOrganisationGuid,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var organisation = await organisationClient.GetOrganisationAsync(cdpOrganisationGuid, cancellationToken);
            return organisation.Roles
                .Select(ToUmPartyRole)
                .ToHashSet();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return new HashSet<CorePartyRole>();
        }
    }

    public async Task<Guid> CreatePersonInviteAsync(
        Guid cdpOrganisationGuid,
        string email,
        string firstName,
        string lastName,
        IReadOnlyList<string> scopes,
        CancellationToken cancellationToken = default)
    {
        var invite = await organisationClient.CreatePersonInviteForServiceAsync(
            cdpOrganisationGuid,
            new InvitePersonToOrganisation(
                email: email,
                firstName: firstName,
                lastName: lastName,
                scopes: scopes.ToList()),
            cancellationToken);

        return invite.Id;
    }

    private static CorePartyRole ToUmPartyRole(OiPartyRole role) => role switch
    {
        OiPartyRole.Buyer          => CorePartyRole.Buyer,
        OiPartyRole.ProcuringEntity => CorePartyRole.ProcuringEntity,
        OiPartyRole.Supplier       => CorePartyRole.Supplier,
        OiPartyRole.Tenderer       => CorePartyRole.Tenderer,
        OiPartyRole.Funder         => CorePartyRole.Funder,
        OiPartyRole.Enquirer       => CorePartyRole.Enquirer,
        OiPartyRole.Payer          => CorePartyRole.Payer,
        OiPartyRole.Payee          => CorePartyRole.Payee,
        OiPartyRole.ReviewBody     => CorePartyRole.ReviewBody,
        OiPartyRole.InterestedParty => CorePartyRole.InterestedParty,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, $"Unknown OI party role: {role}")
    };
}
