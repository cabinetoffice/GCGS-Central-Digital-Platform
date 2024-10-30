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
using CO.CDP.OrganisationInformation;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class UpdateOrganisationUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<Persistence.IOrganisationRepository> _organisationRepositoryMock = new();
    private readonly Mock<IPublisher> _publisher = new();
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _anotherOrganisationId = Guid.NewGuid();
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
                    LegalName = "Illegal",
                    Scheme = "FakeScheme"
                }]
            }
        };
        var organisation = Organisation;
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        var result = await UseCase.Execute((_organisationId, updateOrganisation));

        result.Should().BeTrue();
        _organisationRepositoryMock.Verify(repo => repo.SaveAsync(organisation, AnyOnSave()), Times.Once);
    }

    [Fact]
    public async Task Execute_ItAssignsOtherIdentifierAsPrimaryWhenVatIdentifierAdded()
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
        _organisationRepositoryMock.Verify(repo => repo.SaveAsync(organisation, AnyOnSave()), Times.Once);

        organisation.Identifiers.First(i => i.Scheme == "Other").Primary.Should().BeTrue();
        organisation.Identifiers.First(i => i.Scheme == "VAT").Primary.Should().BeFalse();
    }

    [Fact]
    public async Task Execute_ItAssignsCompaniesHouseIdentifierAsPrimaryWhenPponIdentifierExistsAsPrimary()
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
                    Scheme = AssignIdentifierUseCase.IdentifierSchemes.CompaniesHouse
                }]
            }
        };
        var organisation = OrganisationWithPponIdentifier;
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        var result = await UseCase.Execute((_organisationId, updateOrganisation));

        result.Should().BeTrue();
        _organisationRepositoryMock.Verify(repo => repo.SaveAsync(organisation, AnyOnSave()), Times.Once);

        organisation.Identifiers.First(i => i.Scheme == "GB-PPON").Primary.Should().BeFalse();
        organisation.Identifiers.First(i => i.Scheme == AssignIdentifierUseCase.IdentifierSchemes.CompaniesHouse).Primary.Should().BeTrue();
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
            .ThrowAsync<InvalidUpdateOrganisationCommand.UnknownOrganisationUpdateType>();
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
            .ThrowAsync<InvalidUpdateOrganisationCommand.MissingAdditionalIdentifiers>();
    }

    [Fact]
    public async Task ShouldThrowInvalidUpdateOrganisationCommand_WhenIdentifierAlreadyExists()
    {
        // Arrange
        var command = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.AdditionalIdentifiers,
            Organisation = new OrganisationInfo
            {
                AdditionalIdentifiers = [new OrganisationIdentifier
                {
                    Id = "13294342",
                    LegalName = "Tcme",
                    Scheme = "VAT"
                }]
            }
        };

        var organisation = Organisation;

        _organisationRepositoryMock.Setup(repo => repo.Find(_anotherOrganisationId)).ReturnsAsync(AnotherOrganisation);

        _organisationRepositoryMock.Setup(repo => repo.FindByIdentifier("VAT", "13294342")).ReturnsAsync(organisation);



        Func<Task> act = async () => await UseCase.Execute((_anotherOrganisationId, command));


        await act.Should()
          .ThrowAsync<InvalidUpdateOrganisationCommand>()
            .WithMessage("The identifier you have entered belongs to a different organization that already exists.");
    }

    [Fact]
    public async Task ShouldAddNewIdentifier_WhenIdentifierDoesNotExistInOrganisation()
    {
        // Arrange
        var command = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.AdditionalIdentifiers,
            Organisation = new OrganisationInfo
            {
                AdditionalIdentifiers = [new OrganisationIdentifier
                {
                    Id = "342",
                    LegalName = "Tcme",
                    Scheme = "VAT"
                }]
            }
        };

        var anotherOrganisation = AnotherOrganisation;

        _organisationRepositoryMock.Setup(repo => repo.Find(_anotherOrganisationId)).ReturnsAsync(anotherOrganisation);

        _organisationRepositoryMock.Setup(repo => repo.FindByIdentifier("VAT", "13294342")).ReturnsAsync((Persistence.Organisation?)null);

        var result = await UseCase.Execute((_anotherOrganisationId, command));

        result.Should().BeTrue();

        _organisationRepositoryMock.Verify(repo => repo.SaveAsync(anotherOrganisation, AnyOnSave()), Times.Once);

        // Assert
        anotherOrganisation.Identifiers.Should().ContainSingle(i =>
            i.IdentifierId == "342" &&
            i.Scheme == "VAT" &&
            i.LegalName == "Tcme");
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
            .ThrowAsync<InvalidUpdateOrganisationCommand.MissingOrganisationAddress>();
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

        _organisationRepositoryMock.Verify(repo =>
            repo.SaveAsync(organisation, OnSaveRespondingTo(organisation)), Times.Once);
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
        _organisationRepositoryMock.Verify(repo => repo.SaveAsync(organisation, AnyOnSave()), Times.Once);

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
         .ThrowAsync<InvalidUpdateOrganisationCommand.NoPrimaryIdentifier>();
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
        _organisationRepositoryMock.Verify(repo => repo.SaveAsync(organisation, AnyOnSave()), Times.Once);

        organisation.Identifiers.FirstOrDefault(i =>
            i is { Scheme: "GB-PPON", IdentifierId: "c0777aeb968b4113a27d94e55b10c1b4", Primary: true })
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

        Func<Task> act = async () => await UseCase.Execute((_organisationId, command));

        await act.Should()
           .ThrowAsync<InvalidUpdateOrganisationCommand>();

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
            .ThrowAsync<InvalidUpdateOrganisationCommand.MissingOrganisationName>();
    }

    [Fact]
    public async Task Execute_ShouldUpdateOrganisationName_WhenOrganisationAlreadyExists()
    {
        var command = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.OrganisationName,
            Organisation = new OrganisationInfo
            {
               OrganisationName="Updated Organisation Name"
            }
        };
        var organisation = Organisation;
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        var result = await UseCase.Execute((_organisationId, command));

        result.Should().BeTrue();
        _organisationRepositoryMock.Verify(repo => repo.SaveAsync(organisation, AnyOnSave()), Times.Once);

        organisation.Name.Should().Be("Updated Organisation Name");
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
            .ThrowAsync<InvalidUpdateOrganisationCommand.MissingOrganisationEmail>();
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
            .ThrowAsync<InvalidUpdateOrganisationCommand.OrganisationEmailDoesNotExist>();
    }

    [Fact]
    public async Task Execute_ShouldUpdateOrganisationEmail_WhenOrganisationContactPointExists()
    {
        var command = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.OrganisationEmail,
            Organisation = new OrganisationInfo
            {
                ContactPoint=new OrganisationContactPoint()
                { Email="updatedemail@test.com" }
            }
        };
        var organisation = Organisation;
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        var result = await UseCase.Execute((_organisationId, command));

        result.Should().BeTrue();
        _organisationRepositoryMock.Verify(repo => repo.SaveAsync(organisation, AnyOnSave()), Times.Once);

        organisation.ContactPoints.FirstOrDefault()!.Email.Should().Be("updatedemail@test.com");
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidUpdateOrganisationCommand_WhenOrganisationRegisteredAddressIsNull()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.RegisteredAddress,
            Organisation = new()
        };
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(Organisation);

        Func<Task> act = async () => await UseCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<InvalidUpdateOrganisationCommand.MissingOrganisationAddress>();
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidUpdateOrganisationCommand_WhenOrganisationRegisteredAddressIsMissing()
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
                        CountryName = "Test Land",
                        Country = "AB",
                        Region="Test Region"
                     }
                ]
            }
        };

        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(Organisation);

        Func<Task> act = async () => await UseCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<InvalidUpdateOrganisationCommand.MissingOrganisationRegisteredAddress>();
    }

    [Fact]
    public async Task Execute_ShouldUpdateOrganisationRegisteredAddress_WhenOrganisationExists()
    {
        var command = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.RegisteredAddress,
            Organisation = new OrganisationInfo
            {
                Addresses = [
                    new OrganisationAddress
                    {
                        Type=OrganisationInformation.AddressType.Registered,
                        StreetAddress = "1234 Test St",
                        Locality = "Test City",
                        PostalCode = "12345",
                        CountryName = "Test Land updated",
                        Country = "AB",
                        Region="Test Region"
                     }
                ]
            }
        };
        var organisation = Organisation;
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        var result = await UseCase.Execute((_organisationId, command));

        result.Should().BeTrue();
        _organisationRepositoryMock.Verify(repo => repo.SaveAsync(organisation, AnyOnSave()), Times.Once);

        organisation.Addresses.FirstOrDefault(x=>x.Type==OrganisationInformation.AddressType.Registered)!.Address.CountryName.Should().Be("Test Land updated");
    }

    [Fact]
    public async Task Execute_ShouldUpdateOrganisationRolesToTwo_WhenOrganisationHasOneRole()
    {
        var command = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.AddRoles,
            Organisation = new OrganisationInfo
            {
                Roles = [PartyRole.Tenderer]
            }
        };
        var organisation = Organisation;
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        var result = await UseCase.Execute((_organisationId, command));

        result.Should().BeTrue();
        _organisationRepositoryMock.Verify(repo => repo.SaveAsync(organisation, AnyOnSave()), Times.Once);

        organisation.Roles.Should().BeEquivalentTo([PartyRole.Buyer, PartyRole.Tenderer]);
    }

    [Fact]
    public async Task Execute_ShouldUpdateOrganisationRolesThrowsException_WhenRolesAreMissing()
    {
        var command = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.AddRoles,
            Organisation = new OrganisationInfo
            {
            }
        };
        var organisation = Organisation;
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        Func<Task> act = async () => await UseCase.Execute((_organisationId, command));

        await act.Should()
            .ThrowAsync<InvalidUpdateOrganisationCommand.MissingRoles>();
    }

    [Fact]
    public async Task Execute_ShouldThrowMissingContactPoint_WhenContactPointIsMissing()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.ContactPoint,
            Organisation = new OrganisationInfo()
        };
        var organisation = Organisation;

        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        Func<Task> act = async () => await UseCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<InvalidUpdateOrganisationCommand.MissingContactPoint>()
            .WithMessage("Missing contact point.");
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
                    Scheme = "GB-PPON",
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
                    CountryName = "Test Land",
                    Country = "AB"
                }
            }},
            Roles = [PartyRole.Buyer]
        };

    private Persistence.Organisation AnotherOrganisation =>
     new()
     {
         Guid = _organisationId,
         Name = "Tcme Ltd",
         Tenant = It.IsAny<Persistence.Tenant>(),
         Identifiers = [
                  new Persistence.Organisation.Identifier
                {
                    Scheme = "GB-MPR",
                    IdentifierId = "5656",
                    LegalName = "Unilever Ltd",
                    Primary = true
                }
         ],
         ContactPoints = [new Persistence.Organisation.ContactPoint { Email = "test2@test.com" }],
         Addresses = {new OrganisationInformation.Persistence.Organisation.OrganisationAddress
            {
                Type  = OrganisationInformation.AddressType.Registered,
                Address = new Address
                {
                    StreetAddress = "1234 Port St",
                    Locality = "Port City",
                    PostalCode = "178345",
                    CountryName = "Test Land",
                    Country = "AB"
                }
            }},
         Roles = [PartyRole.Buyer]
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
                    Scheme = "GB-PPON",
                    LegalName = "Acme Ltd",
                    Primary = false,
                    IdentifierId = "c0777aeb968b4113a27d94e55b10c1b4"
                }
            ],
            ContactPoints = [new Persistence.Organisation.ContactPoint { Email = "test@test.com" }]
        };

    private static Func<Persistence.Organisation, Task> AnyOnSave()
    {
        return It.Is<Func<Persistence.Organisation, Task>>(_ => true);
    }

    private static Func<Persistence.Organisation, Task> OnSaveRespondingTo(Persistence.Organisation organisation)
    {
        return It.Is<Func<Persistence.Organisation, Task>>(f => f(organisation).ContinueWith(_ => true).Result);
    }
}