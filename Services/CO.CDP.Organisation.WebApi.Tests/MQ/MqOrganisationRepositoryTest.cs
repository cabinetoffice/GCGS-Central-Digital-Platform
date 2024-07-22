using CO.CDP.Organisation.WebApi.MQ;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;

namespace CO.CDP.Organisation.WebApi.Tests.MQ;

public class MqOrganisationRepositoryTest
{
    private readonly Mock<IOrganisationRepository> _decoratedRepository = new();

    [Fact]
    public void ItDelegatesSaveToTheDecoratedRepository()
    {
        var repository = new MqOrganisationRepository(_decoratedRepository.Object);
        var organisation = GivenOrganisation(guid: Guid.NewGuid());

        repository.Save(organisation);

        _decoratedRepository.Verify(r => r.Save(organisation), Times.Once);
    }

    [Fact]
    public async Task ItDelegatesFindToTheDecoratedRepository()
    {
        var repository = new MqOrganisationRepository(_decoratedRepository.Object);
        var organisation = GivenOrganisation(guid: Guid.NewGuid());

        _decoratedRepository.Setup(r => r.Find(organisation.Guid)).ReturnsAsync(organisation);

        var result = await repository.Find(organisation.Guid);

        result.Should().Be(organisation);
    }

    [Fact]
    public async Task ItDelegatesFindByNameToTheDecoratedRepository()
    {
        var repository = new MqOrganisationRepository(_decoratedRepository.Object);
        var organisation = GivenOrganisation(name: "Acme");

        _decoratedRepository.Setup(r => r.FindByName("Acme")).ReturnsAsync(organisation);

        var result = await repository.FindByName("Acme");

        result.Should().Be(organisation);
    }

    [Fact]
    public async Task ItDelegatesFindByUserUrnToTheDecoratedRepository()
    {
        var repository = new MqOrganisationRepository(_decoratedRepository.Object);
        var organisation = GivenOrganisation();

        _decoratedRepository.Setup(r => r.FindByUserUrn("urn:2024:uk:123")).ReturnsAsync([organisation]);

        var result = await repository.FindByUserUrn("urn:2024:uk:123");

        result.Should().Equal([organisation]);
    }

    [Fact]
    public async Task ItDelegatesFindByIdentifierToTheDecoratedRepository()
    {
        var repository = new MqOrganisationRepository(_decoratedRepository.Object);
        var organisation = GivenOrganisation();

        _decoratedRepository.Setup(r => r.FindByIdentifier("GB-COH", "945234323")).ReturnsAsync(organisation);

        var result = await repository.FindByIdentifier("GB-COH", "945234323");

        result.Should().Be(organisation);
    }

    [Fact]
    public void ItDelegatesDisposeToTheDecoratedRepository()
    {
        var repository = new MqOrganisationRepository(_decoratedRepository.Object);

        repository.Dispose();

        _decoratedRepository.Verify(r => r.Dispose(), Times.Once);
    }

    private OrganisationInformation.Persistence.Organisation GivenOrganisation(
        Guid? guid = null,
        string? name = null,
        List<OrganisationInformation.Persistence.Organisation.Identifier>? identifiers = null
    )
    {
        var theGuid = guid ?? Guid.NewGuid();
        var organisation = new OrganisationInformation.Persistence.Organisation
        {
            Guid = theGuid,
            Name = name ?? $"Organisation {theGuid}",
            Tenant = new Tenant
            {
                Guid = theGuid,
                Name = $"Tenant {theGuid}"
            },
            Identifiers = identifiers ??
            [
                new OrganisationInformation.Persistence.Organisation.Identifier
                {
                    Primary = true,
                    Scheme = "ISO9001",
                    IdentifierId = "1",
                    LegalName = "DefaultLegalName",
                    Uri = "https://default.org"
                },
                new OrganisationInformation.Persistence.Organisation.Identifier
                {
                    Primary = false,
                    Scheme = "ISO14001",
                    IdentifierId = "2",
                    LegalName = "AnotherLegalName",
                    Uri = "https://example.com"
                }
            ],
            Addresses = [],
            ContactPoints = [],
            Roles = [PartyRole.Supplier],
            BuyerInfo = null,
            SupplierInfo = null
        };
        return organisation;
    }
}