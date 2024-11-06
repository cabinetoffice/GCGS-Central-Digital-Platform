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


    [Fact]
    public async Task IsInviteEmailUniqueWithinOrganisation_WhenDoesNotExist_ReturnsTrue()
    {
        using var repository = PersonInviteRepository();

        var alreadyInvitedEmail = "john.doe@example.com";
        var newInviteEmail = "jane.doe@example.com";
        var tenant = GivenTenant();
        var organisation = GivenOrganisation();
        var invite = GivenPersonInvite(guid: organisation.Guid, email: alreadyInvitedEmail, tenant: tenant, organisation: organisation);

        await using var context = postgreSql.OrganisationInformationContext();
        await context.PersonInvites.AddAsync(invite);
        await context.SaveChangesAsync();

        var result = await repository.IsInviteEmailUniqueWithinOrganisation(organisation.Guid, newInviteEmail);

        result.Should().Be(true);
    }

    [Fact]
    public async Task IsInviteEmailUniqueWithinOrganisation_WhenDoesExist_ReturnsFalse()
    {
        using var repository = PersonInviteRepository();

        var inviteEmail = "john.doe@example.com";
        var tenant = GivenTenant();
        var organisation = GivenOrganisation();
        var invite = GivenPersonInvite(guid: organisation.Guid, email: inviteEmail, tenant: tenant, organisation: organisation);

        await using var context = postgreSql.OrganisationInformationContext();
        await context.PersonInvites.AddAsync(invite);
        await context.SaveChangesAsync();

        var result = await repository.IsInviteEmailUniqueWithinOrganisation(organisation.Guid, inviteEmail);

        result.Should().Be(false);
    }

    [Fact]
    public async Task FindPersonInviteByEmail_WhenInviteExists_ReturnsInvite()
    {
        using var repository = PersonInviteRepository();

        var email = "john.doe@example.com";
        var tenant = GivenTenant();
        var organisation = GivenOrganisation(tenant: tenant);
        var invite = GivenPersonInvite(guid: Guid.NewGuid(), email: email, organisation: organisation, tenant: tenant);

        await using var context = postgreSql.OrganisationInformationContext();
        await context.PersonInvites.AddAsync(invite);
        await context.SaveChangesAsync();

        var result = await repository.FindPersonInviteByEmail(organisation.Guid, email);

        var personInvites = result as PersonInvite[] ?? result.ToArray();
        personInvites.Should().NotBeNullOrEmpty();

        personInvites.First().Email.Should().Be(email);
    }


    [Fact]
    public async Task FindPersonInviteByEmail_WhenInviteDoesNotExist_ReturnsEmpty()
    {
        using var repository = PersonInviteRepository();

        var email = "nonexistent@example.com";
        var tenant = GivenTenant();
        var organisation = GivenOrganisation();
        var invite = GivenPersonInvite(guid: Guid.NewGuid(), email: "john.doe@example.com", tenant: tenant, organisation: organisation);

        await using var context = postgreSql.OrganisationInformationContext();
        await context.PersonInvites.AddAsync(invite);
        await context.SaveChangesAsync();

        var result = await repository.FindPersonInviteByEmail(organisation.Guid, email);

        result.Should().BeEmpty();
    }

    private IPersonInviteRepository PersonInviteRepository()
    {
        return new DatabasePersonInviteRepository(postgreSql.OrganisationInformationContext());
    }
}
