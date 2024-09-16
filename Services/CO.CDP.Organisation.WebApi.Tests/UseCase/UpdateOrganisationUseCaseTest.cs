using CO.CDP.MQ;
using CO.CDP.Organisation.WebApi.Events;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.Tests.UseCase.Extensions;
using CO.CDP.Organisation.WebApi.UseCase;
using FluentAssertions;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;
using Address = CO.CDP.OrganisationInformation.Persistence.Address;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class UpdateOrganisationUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<Persistence.IOrganisationRepository> _organisationRepositoryMock = new();
    private readonly Mock<IPublisher> _publisher = new();
    private readonly Guid _organisationId = Guid.NewGuid();
    private UpdateOrganisationUseCase UseCase =>
        new(_organisationRepositoryMock.Object, _publisher.Object, mapperFixture.Mapper);

    [Fact]
    public async Task Execute_ShouldUpdateOrganisation_WhenOrganisationExists()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.AdditionalIdentifiers,
            Organisation = new OrganisationInfo
            {
                AdditionalIdentifiers = [new OrganisationIdentifier
                {
                    Id = "FakeId",
                    LegalName = "Illigal",
                    Scheme = "FakeScheme"
                }]
            }
        };
        var organisation = Organisation;
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        var result = await UseCase.Execute((_organisationId, updateOrganisation));

        result.Should().BeTrue();
        _organisationRepositoryMock.Verify(repo => repo.Save(organisation!), Times.Once);
    }

    [Fact]
    public async Task Execute_ItAssignsVatIdentifierAsPrimaryWhenOtherIdentifierExistsAsPrimary()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.AdditionalIdentifiers,
            Organisation = new OrganisationInfo
            {
                AdditionalIdentifiers = [new OrganisationIdentifier
                {
                    Id = "0123456789",
                    LegalName = "Acme Ltd",
                    Scheme = "VAT"
                }]
            }
        };
        var organisation = OrganisationWithOtherIdentifier;
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        var result = await UseCase.Execute((_organisationId, updateOrganisation));

        result.Should().BeTrue();
        _organisationRepositoryMock.Verify(repo => repo.Save(organisation!), Times.Once);

        organisation.Identifiers.First(i => i.Scheme == "Other").Primary.Should().BeFalse();
        organisation.Identifiers.First(i => i.Scheme == "VAT").Primary.Should().BeTrue();
    }

    [Fact]
    public async Task Execute_ItAssignsVatIdentifierAsPrimaryWhenPponIdentifierExistsAsPrimary()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.AdditionalIdentifiers,
            Organisation = new OrganisationInfo
            {
                AdditionalIdentifiers = [new OrganisationIdentifier
                {
                    Id = "0123456789",
                    LegalName = "Acme Ltd",
                    Scheme = "VAT"
                }]
            }
        };
        var organisation = OrganisationWithPponIdentifier;
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        var result = await UseCase.Execute((_organisationId, updateOrganisation));

        result.Should().BeTrue();
        _organisationRepositoryMock.Verify(repo => repo.Save(organisation!), Times.Once);

        organisation.Identifiers.First(i => i.Scheme == "CDP-PPON").Primary.Should().BeFalse();
        organisation.Identifiers.First(i => i.Scheme == "VAT").Primary.Should().BeTrue();
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownOrganisationException_WhenOrganisationNotFound()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.AdditionalIdentifiers,
            Organisation = new()
        };
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync((Persistence.Organisation?)null);

        Func<Task> act = async () => await UseCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {_organisationId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidUpdateOrganisationCommand_WhenUpdateTypeIsUnknown()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = (OrganisationUpdateType)999, // Unknown type
            Organisation = new()
        };
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(Organisation);

        Func<Task> act = async () => await UseCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<InvalidUpdateOrganisationCommand>()
            .WithMessage("Unknown organisation update type.");
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidUpdateOrganisationCommand_WhenAdditionalIdentifiersIsNull()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.AdditionalIdentifiers,
            Organisation = new()
        };
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(Organisation);

        Func<Task> act = async () => await UseCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<InvalidUpdateOrganisationCommand>()
            .WithMessage("Missing additional identifiers.");
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidUpdateOrganisationCommand_WhenAddressIsNull()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.Address,
            Organisation = new()
        };
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(Organisation);

        Func<Task> act = async () => await UseCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<InvalidUpdateOrganisationCommand>()
            .WithMessage("Missing organisation address.");
    }

    [Fact]
    public async Task Execute_ShouldPublishOrganisationUpdatedEvent_WhenOrganisationIsUpdated()
    {
        var command = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.AdditionalIdentifiers,
            Organisation = new OrganisationInfo
            {
                AdditionalIdentifiers = [new OrganisationIdentifier
                {
                    Id = "999888",
                    LegalName = "Acme",
                    Scheme = "GB-COH"
                }]
            }
        };
        var organisation = Organisation;
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        await UseCase.Execute((_organisationId, command));

        _publisher.Verify(p => p.Publish(It.IsAny<OrganisationUpdated>()), Times.Once);
        _publisher.Invocations[0].Arguments[0].Should().BeEquivalentTo(new OrganisationUpdated
        {
            Id = organisation.Guid.ToString(),
            Name = organisation.Name,
            Identifier = organisation.Identifiers.First(i => i.Primary).AsEventValue(),
            AdditionalIdentifiers = organisation.Identifiers.Where(i => !i.Primary).AsEventValue().ToList(),
            Addresses = organisation.Addresses.AsEventValue(),
            ContactPoint = organisation.ContactPoints.First().AsEventValue(),
            Roles = organisation.Roles.AsEventValue()
        });

    }

    [Fact]
    public async Task Execute_ShouldUpdateVatNumber_WhenVatIdentifierAlreadyExists()
    {
        var command = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.AdditionalIdentifiers,
            Organisation = new OrganisationInfo
            {
                AdditionalIdentifiers = [new OrganisationIdentifier
                {
                    Id = "999999",
                    LegalName = "Acme",
                    Scheme = "VAT"
                }]
            }
        };
        var organisation = Organisation;
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        var result = await UseCase.Execute((_organisationId, command));

        result.Should().BeTrue();
        _organisationRepositoryMock.Verify(repo => repo.Save(organisation!), Times.Once);

        organisation.Identifiers.Should().Contain(i => i.Scheme == "VAT" && i.IdentifierId == "999999");
        organisation.Identifiers.Should().ContainSingle();
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenVatIdentifierIsOnlyIdentifierAndVatNumberIsRemoved()
    {
        var command = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.RemoveIdentifier,
            Organisation = new OrganisationInfo
            {
                IdentifierToRemove = new OrganisationIdentifier
                {
                    Id = "",
                    LegalName = "Acme",
                    Scheme = "VAT"
                }
            }
        };
        var organisation = Organisation;
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        Func<Task> act = async () => await UseCase.Execute((_organisationId, command));

        await act.Should()
         .ThrowAsync<InvalidUpdateOrganisationCommand>()
         .WithMessage("There are no identifiers remaining that can be set as the primary.");
    }

    [Fact]
    public async Task Execute_ShouldAssignPponAsPrimary_WhenVatIdentifierIsRemoved()
    {
        var command = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.RemoveIdentifier,
            Organisation = new OrganisationInfo
            {
                IdentifierToRemove = new OrganisationIdentifier
                {
                    Id = "",
                    LegalName = "Acme",
                    Scheme = "VAT"
                }
            }
        };
        var organisation = OrganisationWithVatPrimaryAndPpon;
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        var result = await UseCase.Execute((_organisationId, command));

        result.Should().BeTrue();
        _organisationRepositoryMock.Verify(repo => repo.Save(organisation!), Times.Once);

        organisation.Identifiers.FirstOrDefault(i =>
            i.Scheme == "CDP-PPON" &&
            i.IdentifierId == "c0777aeb968b4113a27d94e55b10c1b4" &&
            i.Primary)
            .Should().NotBeNull();
        organisation.Identifiers.Should().ContainSingle();
    }

    [Fact]
    public async Task Execute_ShouldNotInsertIdentifier_WhenIdentifierIdIsEmptyOrNull()
    {
        var command = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.AdditionalIdentifiers,
            Organisation = new OrganisationInfo
            {
                AdditionalIdentifiers = [new OrganisationIdentifier
                {
                    Id = "",
                    LegalName = "Acme",
                    Scheme = "VAT"
                },
                new OrganisationIdentifier
                {
                    Id = null,
                    LegalName = "Acme",
                    Scheme = "VAT"
                }]
            }
        };
        var organisation = OrganisationWithPponIdentifier;
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        var result = await UseCase.Execute((_organisationId, command));

        result.Should().BeTrue();
        _organisationRepositoryMock.Verify(repo => repo.Save(organisation!), Times.Once);

        organisation.Identifiers.FirstOrDefault(i => i.Scheme == "VAT").Should().BeNull();
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidUpdateOrganisationCommand_WhenOrganisationNameIsNull()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.OrganisationName,
            Organisation = new()
        };
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(Organisation);

        Func<Task> act = async () => await UseCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<InvalidUpdateOrganisationCommand>()
            .WithMessage("Missing organisation name.");
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidUpdateOrganisationCommand_WhenOrganisationEmailIsNull()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.OrganisationEmail,
            Organisation = new()
        };
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(Organisation);

        Func<Task> act = async () => await UseCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<InvalidUpdateOrganisationCommand>()
            .WithMessage("Missing organisation email.");
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidUpdateOrganisationCommand_WhenOrganisationEmail_DoesNotExists()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.OrganisationEmail,
            Organisation = new OrganisationInfo()
            {
                ContactPoint = new OrganisationContactPoint()
                {
                    Email = "testemail@test.com"
                }
            }
        };
        var organisation = Organisation;
        organisation.ContactPoints = [];

        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        Func<Task> act = async () => await UseCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<InvalidUpdateOrganisationCommand>()
            .WithMessage("organisation email does not exists.");
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidUpdateOrganisationCommand_WhenOrganisationRegsiteredAddressIsNull()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.RegisteredAddress,
            Organisation = new()
        };
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(Organisation);

        Func<Task> act = async () => await UseCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<InvalidUpdateOrganisationCommand>()
            .WithMessage("Missing organisation address.");
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidUpdateOrganisationCommand_WhenOrganisationRegsiteredAddressIsMissing()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.RegisteredAddress,
            Organisation = new OrganisationInfo()
            {
                Addresses = [
                    new OrganisationAddress
                    {
                        Type=OrganisationInformation.AddressType.Postal,                        
                        StreetAddress = "1234 Test St",
                        Locality = "Test City",
                        PostalCode = "12345",
                        CountryName = "Testland",
                        Country = "AB",
                        Region="Test Region"
                     }
                ]
            }
        };

        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(Organisation);

        Func<Task> act = async () => await UseCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<InvalidUpdateOrganisationCommand>()
            .WithMessage("Missing Organisation regsitered address.");
    }




    private Persistence.Organisation OrganisationWithOtherIdentifier =>
        new()
        {
            Guid = _organisationId,
            Name = "Acme Ltd",
            Tenant = It.IsAny<Persistence.Tenant>(),
            Identifiers = [
                new Persistence.Organisation.Identifier
                {
                    Scheme = "Other",
                    LegalName = "Acme Ltd",
                    Primary = true
                }
            ],
            ContactPoints = [new Persistence.Organisation.ContactPoint { Email = "test@test.com" }]
        };

    private Persistence.Organisation OrganisationWithPponIdentifier =>
        new()
        {
            Guid = _organisationId,
            Name = "Acme Ltd",
            Tenant = It.IsAny<Persistence.Tenant>(),
            Identifiers = [
                new Persistence.Organisation.Identifier
                {
                    Scheme = "CDP-PPON",
                    LegalName = "Acme Ltd",
                    Primary = true,
                    IdentifierId = "c0777aeb968b4113a27d94e55b10c1b4"
                }
            ],
            ContactPoints = [new Persistence.Organisation.ContactPoint { Email = "test@test.com" }]
        };

    private Persistence.Organisation Organisation =>
        new()
        {
            Guid = _organisationId,
            Name = "Acme Ltd",
            Tenant = It.IsAny<Persistence.Tenant>(),
            Identifiers = [
                new Persistence.Organisation.Identifier
                {
                    Scheme = "VAT",
                    IdentifierId = "93294342",
                    LegalName = "Acme Ltd",
                    Primary = true
                }
            ],
            ContactPoints = [new Persistence.Organisation.ContactPoint { Email = "test@test.com" }],
            Addresses = {new OrganisationInformation.Persistence.Organisation.OrganisationAddress
            {
                Type  = OrganisationInformation.AddressType.Registered,
                Address = new Address
                {
                    StreetAddress = "1234 Test St",
                    Locality = "Test City",
                    PostalCode = "12345",
                    CountryName = "Testland",
                    Country = "AB"
                }
            }}

        };

    private Persistence.Organisation OrganisationWithVatPrimaryAndPpon =>
        new()
        {
            Guid = _organisationId,
            Name = "Acme Ltd",
            Tenant = It.IsAny<Persistence.Tenant>(),
            Identifiers = [
                new Persistence.Organisation.Identifier
                    {
                        Scheme = "VAT",
                        IdentifierId = "93294342",
                        LegalName = "Acme Ltd",
                        Primary = true
                    },
                new Persistence.Organisation.Identifier
                {
                    Scheme = "CDP-PPON",
                    LegalName = "Acme Ltd",
                    Primary = false,
                    IdentifierId = "c0777aeb968b4113a27d94e55b10c1b4"
                }
            ],
            ContactPoints = [new Persistence.Organisation.ContactPoint { Email = "test@test.com" }]
        };
}