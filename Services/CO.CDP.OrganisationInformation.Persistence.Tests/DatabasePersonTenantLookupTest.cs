using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static CO.CDP.OrganisationInformation.Persistence.Tests.EntityFactory;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class DatabasePersonTenantLookupTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task ItFindsThePersonWithAllTheirTenants()
    {
        using var repository = PersonRepository();

        var acmeCoPersonUrn = "urn:fdc:gov.uk:2022:7wTqYGMFQxgukTSpSI2G0dMwe9";
        var acmeCoTenant = GivenTenant(name: "Acme Co Tenant");
        var acmeCoOrganisation = GivenOrganisation(tenant: acmeCoTenant);
        var acmeCoPerson = GivenPerson(
            userUrn: acmeCoPersonUrn,
            tenant: acmeCoTenant,
            organisationsWithScope: [(acmeCoOrganisation, ["ADMIN"])]
        );
        var acmeCoPersonWithNoTenantConnection = GivenPerson(
            organisationsWithScope: [(acmeCoOrganisation, ["USER"])]
        );
        var acmeCoPersonWithNoOrganisationConnection = GivenPerson(
            tenant: acmeCoTenant
        );

        var widgetCoTenant = GivenTenant(name: "Widget Co Tenant");
        var widgetCoOrganisation = GivenOrganisation(tenant: widgetCoTenant);
        var widgetCoPerson = GivenPerson(
            tenant: widgetCoTenant,
            organisationsWithScope: [(widgetCoOrganisation, ["USER"]), (acmeCoOrganisation, ["USER"])]
        );

        repository.Save(acmeCoPerson);
        repository.Save(widgetCoPerson);
        repository.Save(acmeCoPersonWithNoTenantConnection);
        repository.Save(acmeCoPersonWithNoOrganisationConnection);

        var person = await postgreSql.OrganisationInformationContext()
            .Persons
            .Where(p => p.UserUrn == acmeCoPersonUrn)
            .Include(p => p.Tenants)
            .ThenInclude(t => t.Organisations)
            .ThenInclude(o => o.OrganisationPersons.Where(op => op.Person.UserUrn == acmeCoPersonUrn))
            .SingleAsync();

        person.Guid.Should().Be(acmeCoPerson.Guid);
        person.Tenants.Count.Should().Be(1);
        person.Tenants.Should().Contain(t => t.Name == acmeCoTenant.Name);
        person.Tenants.First().Organisations.Should().Contain(o => o.Guid == acmeCoOrganisation.Guid);
        person.Tenants.First().Organisations.Count.Should().Be(1);
        person.Tenants.First().Organisations.First().OrganisationPersons.Should().Contain(op =>
            op.Person.Guid == acmeCoPerson.Guid &&
            op.Organisation.Guid == acmeCoOrganisation.Guid &&
            op.Scopes.Contains("ADMIN") &&
            op.Scopes.Count == 1
        );
        person.Tenants.First().Organisations.First().OrganisationPersons.Count.Should().Be(1);
    }

    private IPersonRepository PersonRepository()
    {
        return new DatabasePersonRepository(postgreSql.OrganisationInformationContext());
    }
}