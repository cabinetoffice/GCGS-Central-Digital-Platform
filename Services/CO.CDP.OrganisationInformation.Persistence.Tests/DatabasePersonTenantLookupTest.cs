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
        var acmeCoPersonScopes = new List<string> { "ADMIN" };
        var acmeCoTenant = GivenTenant(name: "Acme Co Tenant");
        var acmeCoOrganisation = GivenOrganisation(tenant: acmeCoTenant);
        var acmeCoPerson = GivenPerson(
            userUrn: acmeCoPersonUrn,
            tenant: acmeCoTenant,
            organisationsWithScope: [(acmeCoOrganisation, acmeCoPersonScopes)]
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

        var tenantLookup = await postgreSql.OrganisationInformationContext()
            .Persons
            .Where(p => p.UserUrn == acmeCoPersonUrn)
            .Select(p => new TenantLookup
            {
                User = new TenantLookup.PersonUser
                {
                    Email = p.Email,
                    Urn = p.UserUrn ?? "",
                    Name = $"{p.FirstName} {p.LastName}"
                },
                Tenants = p.Tenants.Select(t => new TenantLookup.Tenant
                {
                    Id = t.Guid,
                    Name = t.Name,
                    Organisations = t.Organisations.Select(o => new TenantLookup.Organisation
                    {
                        Id = o.Guid,
                        Name = o.Name,
                        Roles = o.Roles,
                        Uri = "",
                        Scopes = o.OrganisationPersons.Single(op => op.PersonId == p.Id).Scopes
                    }).ToList()
                }).ToList()
            })
            .SingleAsync();

        tenantLookup.Should().BeEquivalentTo(new TenantLookup
        {
            User = new TenantLookup.PersonUser
            {
                Email = acmeCoPerson.Email,
                Name = $"{acmeCoPerson.FirstName} {acmeCoPerson.LastName}",
                Urn = acmeCoPersonUrn
            },
            Tenants = new List<TenantLookup.Tenant>
            {
                new()
                {
                    Id = acmeCoTenant.Guid,
                    Name = acmeCoTenant.Name,
                    Organisations =
                    [
                        new TenantLookup.Organisation
                        {
                            Id = acmeCoOrganisation.Guid,
                            Name = acmeCoOrganisation.Name,
                            Roles = acmeCoOrganisation.Roles,
                            Scopes = acmeCoPersonScopes,
                            Uri = ""
                        }
                    ]
                }
            }
        });
    }

    private IPersonRepository PersonRepository()
    {
        return new DatabasePersonRepository(postgreSql.OrganisationInformationContext());
    }
}

public class TenantLookup
{
    public required PersonUser User { get; init; }
    public required List<Tenant> Tenants { get; init; }

    public class PersonUser
    {
        public required string Name { get; init; }
        public required string Urn { get; init; }
        public required string Email { get; init; }
    }

    public class Tenant
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public required List<Organisation> Organisations { get; init; }
    }

    public class Organisation
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public required List<PartyRole> Roles { get; init; }
        public required string Uri { get; init; }
        public required List<string> Scopes { get; init; }
    }
}