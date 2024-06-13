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
        var widgetCoPersonUrn = "urn:fdc:gov.uk:2022:6fTvD1cMhQNJxrLZSyBgo5";
        var acmeCoWithNoTenantPersonUrn = "urn:fdc:gov.uk:2022:2jGsPVWXzmR0Nd8qBEaUlY";
        var acmeCoWithNoOrganisationPersonUrn = "urn:fdc:gov.uk:2022:Mwe9xZbHnRcFJLtYv3Kp7Q";

        var adminPersonScopes = new List<string> { "ADMIN" };
        var userPersonScopes = new List<string> { "USER" };

        var acmeCoTenant = GivenTenant(name: "Acme Co Tenant");
        var acmeCoOrganisation = GivenOrganisation(
            name: "Acme Co Organisation",
            tenant: acmeCoTenant);
        var acmeCoPerson = GivenPerson(
            userUrn: acmeCoPersonUrn,
            tenant: acmeCoTenant,
            organisationsWithScope: [(acmeCoOrganisation, adminPersonScopes)]
        );
        var acmeCoPersonWithNoTenantConnection = GivenPerson(
            userUrn: acmeCoWithNoTenantPersonUrn,
            organisationsWithScope: [(acmeCoOrganisation, ["USER"])]
        );
        var acmeCoPersonWithNoOrganisationConnection = GivenPerson(
            userUrn: acmeCoWithNoOrganisationPersonUrn,
            tenant: acmeCoTenant
        );

        var widgetCoTenant = GivenTenant(name: "Widget Co Org");
        var widgetCoOrganisation = GivenOrganisation(
            name: "Widget Co Organisation",
            tenant: widgetCoTenant);
        var widgetCoPerson = GivenPerson(
            userUrn: widgetCoPersonUrn,
            tenant: widgetCoTenant,
            organisationsWithScope: [(widgetCoOrganisation, ["USER"]), (acmeCoOrganisation, ["USER"])]
        );

        personRepository.Save(acmeCoPerson);
        personRepository.Save(widgetCoPerson);
        personRepository.Save(acmeCoPersonWithNoTenantConnection);
        personRepository.Save(acmeCoPersonWithNoOrganisationConnection);

        var acmeCoPersonLookup = await tenantRepository.LookupTenant(acmeCoPersonUrn);
        acmeCoPersonLookup.Should().BeEquivalentTo(new TenantLookup
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
                            Scopes = adminPersonScopes
                        }
                    ]
                }
            ]
        });


        var widgetCoPersonLookup = await tenantRepository.LookupTenant(widgetCoPersonUrn);
        widgetCoPersonLookup.Should().BeEquivalentTo(new TenantLookup
        {
            User = new TenantLookup.PersonUser
            {
                Email = widgetCoPerson.Email,
                Name = $"{widgetCoPerson.FirstName} {widgetCoPerson.LastName}",
                Urn = widgetCoPersonUrn
            },
            Tenants =
            [
                new()
                {
                    Id = widgetCoTenant.Guid,
                    Name = widgetCoTenant.Name,
                    Organisations =
                    [
                        new TenantLookup.Organisation
                        {
                            Id = widgetCoOrganisation.Guid,
                            Name = widgetCoOrganisation.Name,
                            Roles = widgetCoOrganisation.Roles,
                            Scopes = userPersonScopes
                        }
                    ]
                }
            ]
        });


        var acmeCoPersonWithNoTenantConnectionLookup = await tenantRepository.LookupTenant(acmeCoWithNoTenantPersonUrn);
        acmeCoPersonWithNoTenantConnectionLookup.Should().BeEquivalentTo(new TenantLookup
        {
            User = new TenantLookup.PersonUser
            {
                Email = acmeCoPersonWithNoTenantConnection.Email,
                Name = $"{acmeCoPersonWithNoTenantConnection.FirstName} {acmeCoPersonWithNoTenantConnection.LastName}",
                Urn = acmeCoWithNoTenantPersonUrn
            },
            Tenants = new List<TenantLookup.Tenant>()
        });


        var acmeCoPersonWithNoOrganisationConnectionLookup = await tenantRepository.LookupTenant(acmeCoWithNoOrganisationPersonUrn);
        acmeCoPersonWithNoOrganisationConnectionLookup.Should().BeEquivalentTo(new TenantLookup
        {
            User = new TenantLookup.PersonUser
            {
                Email = acmeCoPersonWithNoOrganisationConnection.Email,
                Name = $"{acmeCoPersonWithNoOrganisationConnection.FirstName} {acmeCoPersonWithNoOrganisationConnection.LastName}",
                Urn = acmeCoWithNoOrganisationPersonUrn
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
                            Scopes = null
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