using CO.CDP.Organisation.WebApi.DapsRegistry;
using CO.CDP.Organisation.WebApi.UseCase;
using FluentAssertions;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class GetUserClaimsComposeTest
{
    private static readonly Guid Council = Guid.Parse("8c1f2d4e-0000-4000-8000-000000000001");

    private static DapsOrganisationClaims Registry(Guid? cdpOrg, params DapsApplicationClaims[] apps) =>
        new("6b06afe8263672a0ceea90aa", "Registry org", cdpOrg?.ToString("D"), apps);

    private static DapsApplicationClaims App(string discriminator, string[] roles, string[] permissions) =>
        new("6b06afe8263672a0ceea90c8", discriminator, $"App {discriminator}", roles, permissions);

    [Fact]
    public void ItTakesOrganisationAndRoleFromCdpAndApplicationRolesFromTheRegistry()
    {
        var claim = GetUserClaimsUseCase.ComposeOrganisation(Council, "Example Borough Council", ["EDITOR", "ADMIN"],
            [Registry(Council, App("find-a-tender", ["Publisher"], ["can-publish"]))]);

        claim.OrganisationId.Should().Be(Council);
        claim.OrganisationName.Should().Be("Example Borough Council");
        claim.OrganisationRole.Should().Be("ADMIN");
        var app = claim.Applications.Should().ContainSingle().Subject;
        app.ClientId.Should().Be("find-a-tender");
        app.ApplicationName.Should().Be("App find-a-tender");
        app.ApplicationId.Should().Be(Guid.Parse("000000006b06afe8263672a0ceea90c8"));
        app.Roles.Should().Equal("Publisher");
        app.Permissions.Should().Equal("can-publish");
    }

    [Fact]
    public void ItIgnoresRegistryOrganisationsForOtherOrNoCdpOrganisations()
    {
        var claim = GetUserClaimsUseCase.ComposeOrganisation(Council, "Council", ["VIEWER"],
            [Registry(Guid.NewGuid(), App("other", ["R"], ["p"])), Registry(null, App("unlinked", ["R"], ["p"]))]);

        claim.Applications.Should().BeEmpty();
        claim.OrganisationRole.Should().Be("VIEWER");
    }

    [Fact]
    public void ItStillReturnsTheOrganisationWhenTheRegistryIsUnavailable()
    {
        var claim = GetUserClaimsUseCase.ComposeOrganisation(Council, "Council", ["ADMIN"], null);

        claim.OrganisationRole.Should().Be("ADMIN");
        claim.Applications.Should().BeEmpty();
    }

    [Fact]
    public void ItMergesRolesWhenTwoRegistryOrganisationsMirrorTheSameCdpOrganisation()
    {
        var claim = GetUserClaimsUseCase.ComposeOrganisation(Council, "Council", ["EDITOR"],
        [
            Registry(Council, App("fts", ["Editor"], ["can-edit"])),
            Registry(Council, App("fts", ["Publisher"], ["can-edit", "can-publish"]))
        ]);

        var app = claim.Applications.Should().ContainSingle().Subject;
        app.Roles.Should().BeEquivalentTo("Editor", "Publisher");
        app.Permissions.Should().BeEquivalentTo("can-edit", "can-publish");
    }

    [Theory]
    [InlineData("6b06afe8263672a0ceea90c8", "00000000-6b06-afe8-2636-72a0ceea90c8")]
    [InlineData("a1000000-0000-4000-8000-000000000001", "a1000000-0000-4000-8000-000000000001")]
    public void ItMapsRegistryIdsToStableGuids(string registryId, string expected)
    {
        GetUserClaimsUseCase.ToGuid(registryId).Should().Be(Guid.Parse(expected));
        GetUserClaimsUseCase.ToGuid("not-hex").Should().Be(GetUserClaimsUseCase.ToGuid("not-hex"));
    }
}
