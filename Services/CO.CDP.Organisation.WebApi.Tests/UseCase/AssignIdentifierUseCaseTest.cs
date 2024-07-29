using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using FluentAssertions;
using Moq;
using static CO.CDP.Organisation.WebApi.UseCase.AssignIdentifierUseCase.AssignIdentifierException;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class AssignIdentifierUseCaseTest
{
    private readonly Mock<Persistence.IOrganisationRepository> _organisations = new();
    private AssignIdentifierUseCase UseCase => new(_organisations.Object);

    [Fact]
    public async Task ItThrowsAnExceptionIfOrganisationIsNotFound()
    {
        var unknownOrganisationId = Guid.NewGuid();

        await UseCase.Invoking(u => u.Execute(new AssignOrganisationIdentifier
        {
            OrganisationId = unknownOrganisationId,
            Identifier = new OrganisationIdentifier
            {
                Id = "c0777aeb968b4113a27d94e55b10c1b4",
                Scheme = "CDP-PPON",
                LegalName = "Acme Ltd"
            }
        })).Should().ThrowAsync<OrganisationNotFoundException>();
    }

    [Fact]
    public async Task ItAssignsPrimaryIdentifierIfOrganisationHasNoIdentifiersAssigned()
    {
        var organisation = GivenOrganisationExist(
            organisationId: Guid.NewGuid(),
            identifiers: []);

        var result = await UseCase.Execute(new AssignOrganisationIdentifier
        {
            OrganisationId = organisation.Guid,
            Identifier = new OrganisationIdentifier
            {
                Id = "c0777aeb968b4113a27d94e55b10c1b4",
                Scheme = "CDP-PPON",
                LegalName = "Acme Ltd"
            }
        });

        _organisations.Verify(r => r.Save(It.Is<Persistence.Organisation>(o =>
            o.Guid == organisation.Guid && o.Identifiers.Contains(new Persistence.Organisation.Identifier
                {
                    Primary = true,
                    Scheme = "CDP-PPON",
                    IdentifierId = "c0777aeb968b4113a27d94e55b10c1b4",
                    LegalName = "Acme Ltd"
                }
            ))));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ItAssignsNonPrimaryIdentifierIfTheOrganisationAlreadyHasIdentifiersAssigned()
    {
        var organisation = GivenOrganisationExist(
            organisationId: Guid.NewGuid(),
            identifiers:
            [
                new Persistence.Organisation.Identifier
                {
                    Primary = true,
                    Scheme = "GB-COH",
                    IdentifierId = "944432342",
                    LegalName = "Acme Ltd"
                }
            ]);

        var result = await UseCase.Execute(new AssignOrganisationIdentifier
        {
            OrganisationId = organisation.Guid,
            Identifier = new OrganisationIdentifier
            {
                Id = "c0777aeb968b4113a27d94e55b10c1b4",
                Scheme = "CDP-PPON",
                LegalName = "Acme Ltd"
            }
        });

        _organisations.Verify(r => r.Save(It.Is<Persistence.Organisation>(o =>
            o.Guid == organisation.Guid && o.Identifiers.Contains(new Persistence.Organisation.Identifier
                {
                    Primary = false,
                    Scheme = "CDP-PPON",
                    IdentifierId = "c0777aeb968b4113a27d94e55b10c1b4",
                    LegalName = "Acme Ltd"
                }
            ))));
        result.Should().BeTrue();
    }

    private Persistence.Organisation GivenOrganisationExist(
        Guid organisationId,
        List<Persistence.Organisation.Identifier>? identifiers = null)
    {
        var organisation = new Persistence.Organisation
        {
            Name = "Acme Ltd",
            Guid = organisationId,
            Identifiers = identifiers ?? [],
            Addresses = [],
            SupplierInfo = new Persistence.Organisation.SupplierInformation(),
            Tenant = new Persistence.Tenant
            {
                Guid = Guid.Parse("68a9d5a1-4330-49c9-be64-7d5a8e8ae18d"),
                Name = "Tenant"
            }
        };

        _organisations.Setup(repo => repo.Find(organisationId)).ReturnsAsync(organisation);

        return organisation;
    }
}