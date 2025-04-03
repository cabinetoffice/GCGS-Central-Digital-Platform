using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static CO.CDP.OrganisationInformation.Persistence.Tests.EntityFactory;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class DatabaseMouRepositoryTests(PostgreSqlFixture postgreSql)
    : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task GetMouReminderOrganisations_ReturnList_WhenMouSignPending()
    {
        using var repository = MouRepository();
        using var context = GetDbContext();

        var mou1 = new Mou { Guid = Guid.NewGuid(), FilePath = "file-version-1.pdf" }; // old-version mou
        var mou2 = new Mou { Guid = Guid.NewGuid(), FilePath = "file-version-2.pdf" };

        // Case 1 - mou not signed
        var org1 = GivenOrganisation(guid: Guid.NewGuid(), personsWithScope: [(GivenPerson(), ["ADMIN"])], roles: [PartyRole.Buyer]);

        // Case 2 - mou signed but with old version
        var person2 = GivenPerson();
        var org2 = GivenOrganisation(guid: Guid.NewGuid(), personsWithScope: [(person2, ["ADMIN"])], roles: [PartyRole.Buyer]);

        // Case 3 - mou signed
        var person3 = GivenPerson();
        var org3 = GivenOrganisation(guid: Guid.NewGuid(), personsWithScope: [(person3, ["ADMIN"])], roles: [PartyRole.Buyer]);

        // Case 4 - mou not signed & reminder sent less than 7 days ago
        var org4 = GivenOrganisation(guid: Guid.NewGuid(), personsWithScope: [(GivenPerson(), ["ADMIN"])], roles: [PartyRole.Buyer]);

        // Case 5 - mou not signed & reminder sent more than 7 days ago
        var org5 = GivenOrganisation(guid: Guid.NewGuid(), personsWithScope: [(GivenPerson(), ["ADMIN"])], roles: [PartyRole.Buyer]);

        await context.Organisations.AddAsync(org1);
        await context.Organisations.AddAsync(org2);
        await context.Organisations.AddAsync(org3);
        await context.Organisations.AddAsync(org4);
        await context.Organisations.AddAsync(org5);
        await context.Mou.AddAsync(mou1);
        await context.Mou.AddAsync(mou2);
        await context.MouSignature.AddAsync(CreateMoUSignature(mou1, person2, org2));
        await context.MouSignature.AddAsync(CreateMoUSignature(mou2, person3, org3));
        await context.MouEmailReminders.AddAsync(new MouEmailReminder { Organisation = org4, ReminderSentOn = DateTimeOffset.UtcNow.AddDays(-1) });
        await context.MouEmailReminders.AddAsync(new MouEmailReminder { Organisation = org5, ReminderSentOn = DateTimeOffset.UtcNow.AddDays(-8) });
        await context.SaveChangesAsync();
        mou1.CreatedOn = DateTime.UtcNow.AddDays(-7);
        await context.SaveChangesAsync();

        var reminders = await repository.GetMouReminderOrganisations(7);

        // Should send reminder for Case 1, 2 & 5
        reminders.Should().HaveCount(3);
        reminders.Select(o => o.Guid).Intersect([org1.Guid, org2.Guid, org5.Guid]).Count().Should().Be(3);
    }

    [Fact]
    public async Task UpsertMouEmailReminder_Create_WhenDoesNotExists()
    {
        using var repository = MouRepository();
        using var context = GetDbContext();

        var org = GivenOrganisation(guid: Guid.NewGuid(), personsWithScope: [(GivenPerson(), ["ADMIN"])], roles: [PartyRole.Buyer]);
        await context.Organisations.AddAsync(org);
        await context.SaveChangesAsync();

        await repository.UpsertMouEmailReminder(org.Id);

        context.MouEmailReminders.Count(mer => mer.OrganisationId == org.Id).Should().Be(1);
    }

    [Fact]
    public async Task UpsertMouEmailReminder_Update_WhenExists()
    {
        using var repository = MouRepository();
        using var context = GetDbContext();

        var org = GivenOrganisation(guid: Guid.NewGuid(), personsWithScope: [(GivenPerson(), ["ADMIN"])], roles: [PartyRole.Buyer]);
        await context.Organisations.AddAsync(org);
        await context.MouEmailReminders.AddAsync(new MouEmailReminder { Organisation = org, ReminderSentOn = DateTimeOffset.UtcNow });
        await context.SaveChangesAsync();

        await repository.UpsertMouEmailReminder(org.Id);

        context.MouEmailReminders.Count(mer => mer.OrganisationId == org.Id).Should().Be(1);
    }

    private static MouSignature CreateMoUSignature(Mou mou, Person person, Organisation org)
    {
        return new MouSignature
        {
            SignatureGuid = Guid.NewGuid(),
            OrganisationId = org.Id,
            Organisation = org,
            CreatedById = person.Id,
            CreatedBy = person,
            Name = "person",
            JobTitle = "Manager",
            MouId = mou.Id,
            Mou = mou
        };
    }

    private DatabaseMouRepository MouRepository() => new(GetDbContext());

    private OrganisationInformationContext? context = null;

    private OrganisationInformationContext GetDbContext()
    {
        return context ?? postgreSql.OrganisationInformationContext();
    }
}