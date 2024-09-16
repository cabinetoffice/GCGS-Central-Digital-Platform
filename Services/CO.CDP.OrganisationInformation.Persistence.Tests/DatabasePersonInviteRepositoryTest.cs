using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using static CO.CDP.OrganisationInformation.Persistence.Tests.EntityFactory;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class DatabasePersonInviteRepositoryTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{

    [Fact]
    public async Task ItFindsSavedInvite()
    {
        using var repository = PersonInviteRepository();

        var tenant = GivenTenant();
        var organisation = GivenOrganisation();
        var invite = GivenPersonInvite(guid: Guid.NewGuid(), tenant: tenant, organisation: organisation);

        repository.Save(invite);

        var found = await repository.Find(invite.Guid);

        found.Should().Be(invite);
        found.As<PersonInvite>().Id.Should().BePositive();
        found.As<PersonInvite>().Organisation.Should().Be(organisation);
        found.As<PersonInvite>().InviteSentOn.Should().BeCloseTo(invite.InviteSentOn, precision: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task ItReturnsNullIfInviteIsNotFound()
    {
        using var repository = PersonInviteRepository();

        var found = await repository.Find(Guid.NewGuid());

        found.Should().BeNull();
    }

    private IPersonInviteRepository PersonInviteRepository()
    {
        return new DatabasePersonInviteRepository(postgreSql.OrganisationInformationContext());
    }
}
