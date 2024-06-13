using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using static CO.CDP.OrganisationInformation.Persistence.Tests.EntityFactory;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class DatabasePersonTenantLookupTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task ItFindsThePersonWithAllTheirTenants()
    {
        using var personRepository = PersonRepository();
        using var tenantRepository = TenantRepository();

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

        personRepository.Save(acmeCoPerson);
        personRepository.Save(widgetCoPerson);
        personRepository.Save(acmeCoPersonWithNoTenantConnection);
        personRepository.Save(acmeCoPersonWithNoOrganisationConnection);

        var tenantLookup = await tenantRepository.LookupTenant(acmeCoPersonUrn);

        tenantLookup.Should().BeEquivalentTo(new TenantLookup
        {
            User = new TenantLookup.PersonUser
            {
                Email = acmeCoPerson.Email,
                Name = $"{acmeCoPerson.FirstName} {acmeCoPerson.LastName}",
                Urn = acmeCoPersonUrn
            },
            Tenants =
            [
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
                            Scopes = acmeCoPersonScopes
                        }
                    ]
                }
            ]
        });
    }

    private ITenantRepository TenantRepository()
    {
        return new DatabaseTenantRepository(postgreSql.OrganisationInformationContext());
    }

    private IPersonRepository PersonRepository()
    {
        return new DatabasePersonRepository(postgreSql.OrganisationInformationContext());
    }
}