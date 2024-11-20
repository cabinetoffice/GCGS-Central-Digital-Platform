using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.MQ;
using CO.CDP.Organisation.WebApi.Events;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.Tests.UseCase.Extensions;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Address = CO.CDP.OrganisationInformation.Persistence.Address;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class UpdateOrganisationUseCaseTest : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<Persistence.IOrganisationRepository> _organisationRepositoryMock = new();
    private readonly Mock<IPublisher> _publisher = new();
    private readonly IConfiguration _configuration;
    private readonly Mock<IGovUKNotifyApiClient> _notifyClient = new();
    private readonly Mock<ILogger<UpdateOrganisationUseCase>> _logger = new();
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _anotherOrganisationId = Guid.NewGuid();
    private readonly AutoMapperFixture _mapperFixture;
    private readonly UpdateOrganisationUseCase _useCase;

    public UpdateOrganisationUseCaseTest(AutoMapperFixture mapperFixture)
    {
        var inMemorySettings = new List<KeyValuePair<string, string?>>
        {
            new("OrganisationAppUrl", "http://baseurl/"),
            new("GOVUKNotify:RequestReviewApplicationEmailTemplateId", "template-id"),
            new("GOVUKNotify:SupportAdminEmailAddress", "admin@example.com")
        };

        _mapperFixture = mapperFixture;

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _useCase = new(_organisationRepositoryMock.Object, _publisher.Object, _mapperFixture.Mapper, _configuration, _notifyClient.Object, _logger.Object);
    }

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

        var organisation = GivenOrganisation([PartyRole.Buyer]);
        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(organisation);

        var result = await _useCase.Execute((_organisationId, updateOrganisation));

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
        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(organisation);

        var result = await _useCase.Execute((_organisationId, updateOrganisation));

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
        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(organisation);

        var result = await _useCase.Execute((_organisationId, updateOrganisation));

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
        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync((Persistence.Organisation?)null);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {_organisationId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownOrganisationUpdateType_WhenUpdateTypeIsUnknown()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = (OrganisationUpdateType)999, // Unknown type
            Organisation = new()
        };

        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(GivenOrganisation([PartyRole.Buyer]));

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<InvalidUpdateOrganisationCommand.UnknownOrganisationUpdateType>();
    }

    [Fact]
    public async Task Execute_ShouldThrowMissingAdditionalIdentifiers_WhenAdditionalIdentifiersIsNull()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.AdditionalIdentifiers,
            Organisation = new()
        };

        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(GivenOrganisation([PartyRole.Buyer]));

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<InvalidUpdateOrganisationCommand.MissingAdditionalIdentifiers>();
    }

    [Fact]
    public async Task ShouldThrowIdentiferNumberAlreadyExists_WhenIdentifierAlreadyExists()
    {
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

        var existingOrganisationWithSameIdentifier = GivenOrganisation([PartyRole.Buyer]);

        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_anotherOrganisationId)).ReturnsAsync(AnotherOrganisation);

        _organisationRepositoryMock.Setup(repo => repo.FindByIdentifier("VAT", "13294342")).ReturnsAsync(existingOrganisationWithSameIdentifier);


        Func<Task> act = async () => await _useCase.Execute((_anotherOrganisationId, command));

        await act.Should()
          .ThrowAsync<InvalidUpdateOrganisationCommand.IdentiferNumberAlreadyExists>();
    }

    [Fact]
    public async Task ShouldAddNewIdentifier_WhenIdentifierDoesNotExistInOrganisation()
    {
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

        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_anotherOrganisationId)).ReturnsAsync(anotherOrganisation);

        _organisationRepositoryMock.Setup(repo => repo.FindByIdentifier("VAT", "13294342")).ReturnsAsync((Persistence.Organisation?)null);

        var result = await _useCase.Execute((_anotherOrganisationId, command));

        result.Should().BeTrue();

        _organisationRepositoryMock.Verify(repo => repo.SaveAsync(anotherOrganisation, AnyOnSave()), Times.Once);

        anotherOrganisation.Identifiers.Should().ContainSingle(i =>
            i.IdentifierId == "342" &&
            i.Scheme == "VAT" &&
            i.LegalName == "Tcme");
    }

    [Fact]
    public async Task Execute_ShouldThrowMissingOrganisationAddress_WhenAddressIsNull()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.Address,
            Organisation = new()
        };

        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(GivenOrganisation([PartyRole.Buyer]));

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateOrganisation));

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

        var organisation = GivenOrganisation([PartyRole.Buyer]);
        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(organisation);

        await _useCase.Execute((_organisationId, command));

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

        var organisation = GivenOrganisation([PartyRole.Buyer]);
        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(organisation);

        var result = await _useCase.Execute((_organisationId, command));

        result.Should().BeTrue();
        _organisationRepositoryMock.Verify(repo => repo.SaveAsync(organisation, AnyOnSave()), Times.Once);

        organisation.Identifiers.Should().Contain(i => i.Scheme == "VAT" && i.IdentifierId == "999999");
        organisation.Identifiers.Should().ContainSingle();
    }

    [Fact]
    public async Task Execute_ShouldThrowExceptionNoPrimaryIdentifier_WhenVatIdentifierIsOnlyIdentifierAndVatNumberIsRemoved()
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

        var organisation = GivenOrganisation([PartyRole.Buyer]);
        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(organisation);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, command));

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
        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(organisation);

        var result = await _useCase.Execute((_organisationId, command));

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
        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(organisation);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, command));

        await act.Should()
           .ThrowAsync<InvalidUpdateOrganisationCommand.MissingIdentifierNumber>();

        organisation.Identifiers.FirstOrDefault(i => i.Scheme == "VAT").Should().BeNull();
    }

    [Fact]
    public async Task Execute_ShouldThrowMissingOrganisationName_WhenOrganisationNameIsNull()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.OrganisationName,
            Organisation = new()
        };

        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(GivenOrganisation([PartyRole.Buyer]));

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<InvalidUpdateOrganisationCommand.MissingOrganisationName>();
    }

    [Fact]
    public async Task Execute_ShouldUpdateOrganisationNameAndTenantAndIdentifiersNames_WhenOrganisationAlreadyExists()
    {
        var command = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.OrganisationName,
            Organisation = new OrganisationInfo
            {
                OrganisationName = "Updated Organisation Name"
            }
        };

        var organisation = GivenOrganisation([PartyRole.Buyer]);

        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(organisation);

        var result = await _useCase.Execute((_organisationId, command));

        result.Should().BeTrue();
        _organisationRepositoryMock.Verify(repo => repo.SaveAsync(organisation, AnyOnSave()), Times.Once);

        organisation.Name.Should().Be("Updated Organisation Name");
        organisation.Tenant.Name.Should().Be("Updated Organisation Name");
        organisation.Identifiers.Select(x => x.LegalName.Should().Be("Updated Organisation Name"));
    }


    [Fact]
    public async Task Execute_ShouldThrowMissingOrganisationEmail_WhenOrganisationEmailIsNull()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.OrganisationEmail,
            Organisation = new()
        };

        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(GivenOrganisation([PartyRole.Buyer]));

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<InvalidUpdateOrganisationCommand.MissingOrganisationEmail>();
    }

    [Fact]
    public async Task Execute_ShouldThrowOrganisationEmailDoesNotExist_WhenOrganisationEmail_DoesNotExists()
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
        var organisation = GivenOrganisation([PartyRole.Buyer]);
        organisation.ContactPoints = [];

        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(organisation);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateOrganisation));

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
                ContactPoint = new OrganisationContactPoint()
                { Email = "updatedemail@test.com" }
            }
        };

        var organisation = GivenOrganisation([PartyRole.Buyer]);
        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(organisation);

        var result = await _useCase.Execute((_organisationId, command));

        result.Should().BeTrue();
        _organisationRepositoryMock.Verify(repo => repo.SaveAsync(organisation, AnyOnSave()), Times.Once);

        organisation.ContactPoints.FirstOrDefault()!.Email.Should().Be("updatedemail@test.com");
    }

    [Fact]
    public async Task Execute_ShouldThrowMissingOrganisationAddress_WhenOrganisationRegisteredAddressIsNull()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.RegisteredAddress,
            Organisation = new()
        };

        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(GivenOrganisation([PartyRole.Buyer]));

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<InvalidUpdateOrganisationCommand.MissingOrganisationAddress>();
    }

    [Fact]
    public async Task Execute_ShouldThrowMissingOrganisationRegisteredAddress_WhenOrganisationRegisteredAddressIsMissing()
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

        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(GivenOrganisation([PartyRole.Buyer]));

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateOrganisation));

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

        var organisation = GivenOrganisation([PartyRole.Buyer]);
        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(organisation);

        var result = await _useCase.Execute((_organisationId, command));

        result.Should().BeTrue();
        _organisationRepositoryMock.Verify(repo => repo.SaveAsync(organisation, AnyOnSave()), Times.Once);

        organisation.Addresses.FirstOrDefault(x => x.Type == OrganisationInformation.AddressType.Registered)!.Address.CountryName.Should().Be("Test Land updated");
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

        var organisation = GivenOrganisation([PartyRole.Buyer]);
        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(organisation);

        var result = await _useCase.Execute((_organisationId, command));

        result.Should().BeTrue();
        _organisationRepositoryMock.Verify(repo => repo.SaveAsync(organisation, AnyOnSave()), Times.Once);

        organisation.Roles.Should().BeEquivalentTo([PartyRole.Buyer, PartyRole.Tenderer]);
    }

    [Fact]
    public async Task Execute_ShouldUpdateOrganisation_WhenOrganisationHasAddAsBuyerRole()
    {
        var command = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.AddAsBuyerRole,
            Organisation = new OrganisationInfo
            {
                BuyerInformation = new BuyerInformation
                {
                    BuyerType = "A buyer type",
                    DevolvedRegulations = []
                }
            }
        };

        var organisation = GivenOrganisation([PartyRole.Tenderer]);
        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(organisation);

        var result = await _useCase.Execute((_organisationId, command));

        result.Should().BeTrue();

        _organisationRepositoryMock.Verify(repo => repo.SaveAsync(organisation, AnyOnSave()), Times.Once);
        organisation.BuyerInfo?.BuyerType.Should().BeEquivalentTo("A buyer type");

        organisation.PendingRoles.Should().ContainSingle().Which.Should().Be(PartyRole.Buyer);

        organisation.Roles.Distinct().Should().BeEquivalentTo(new[] { PartyRole.Tenderer });

        _notifyClient.Verify(n => n.SendEmail(It.IsAny<EmailNotificationRequest>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldUpdateOrganisationRolesThrowsExceptionMissingRoles_WhenRolesAreMissing()
    {
        var command = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.AddRoles,
            Organisation = new OrganisationInfo
            {
            }
        };

        var organisation = GivenOrganisation([PartyRole.Buyer]);
        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(organisation);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, command));

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
        var organisation = GivenOrganisation([PartyRole.Buyer]);

        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(_organisationId)).ReturnsAsync(organisation);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<InvalidUpdateOrganisationCommand.MissingContactPoint>()
            .WithMessage("Missing contact point.");
    }

    [Fact]
    public async Task Execute_ShouldResetRejectedStatus_WhenRejectedBuyerChangesName()
    {
        var organisation = GivenOrganisation(pendingRoles: [PartyRole.Buyer], reviewedBy: GivenPerson(), reviewComment: "Terrible");

        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.OrganisationName,
            Organisation = new OrganisationInfo
            {
                OrganisationName = "Updated Name"
            }
        };

        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(It.IsAny<Guid>())).ReturnsAsync(organisation);
        _organisationRepositoryMock.Setup(repo => repo.FindIncludingReviewedBy(It.IsAny<Guid>())).ReturnsAsync(organisation);

        var result = await _useCase.Execute((Guid.NewGuid(), updateOrganisation));

        result.Should().BeTrue();
        organisation.ReviewedBy.Should().BeNull();
        organisation.ReviewComment.Should().BeNull();

        _organisationRepositoryMock.Verify(repo => repo.SaveAsync(organisation, AnyOnSave()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldResetRejectedStatus_WhenRejectedBuyerChangesEmail()
    {
        var organisation = GivenOrganisation(pendingRoles: [PartyRole.Buyer], reviewedBy: GivenPerson(), reviewComment: "Terrible");

        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.OrganisationEmail,
            Organisation = new OrganisationInfo
            {
                ContactPoint = new OrganisationContactPoint { Email = "foo@bar.com" }
            }
        };

        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(It.IsAny<Guid>())).ReturnsAsync(organisation);
        _organisationRepositoryMock.Setup(repo => repo.FindIncludingReviewedBy(It.IsAny<Guid>())).ReturnsAsync(organisation);

        var result = await _useCase.Execute((Guid.NewGuid(), updateOrganisation));

        result.Should().BeTrue();
        organisation.ReviewedBy.Should().BeNull();
        organisation.ReviewComment.Should().BeNull();

        _organisationRepositoryMock.Verify(repo => repo.SaveAsync(organisation, AnyOnSave()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldNotResetRejectedStatus_WhenRejectedBuyerChangesSomethingOtherThanNameOrEmail()
    {
        var organisation = GivenOrganisation(pendingRoles: [PartyRole.Buyer], reviewedBy: GivenPerson(), reviewComment: "Terrible");

        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.RegisteredAddress,
            Organisation = new OrganisationInfo
            {
                Addresses = [new OrganisationAddress { Country = "UK", CountryName = "UK", Locality = "Devon", PostalCode = "PL1 1LP", StreetAddress = "1 streety street", Type = AddressType.Registered }]
            }
        };

        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(It.IsAny<Guid>())).ReturnsAsync(organisation);

        var result = await _useCase.Execute((Guid.NewGuid(), updateOrganisation));

        result.Should().BeTrue();
        organisation.ReviewedBy.Should().BeEquivalentTo(GivenPerson());
        organisation.ReviewComment.Should().Be("Terrible");

        _organisationRepositoryMock.Verify(repo => repo.SaveAsync(organisation, AnyOnSave()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldSendEmail_WhenRejectedBuyerChangesName()
    {
        var organisation = GivenOrganisation(pendingRoles: [PartyRole.Buyer], reviewedBy: GivenPerson(), reviewComment: "Terrible");

        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.OrganisationName,
            Organisation = new OrganisationInfo
            {
                OrganisationName = "Updated Name"
            }
        };

        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(It.IsAny<Guid>())).ReturnsAsync(organisation);
        _organisationRepositoryMock.Setup(repo => repo.FindIncludingReviewedBy(It.IsAny<Guid>())).ReturnsAsync(organisation);

        var result = await _useCase.Execute((Guid.NewGuid(), updateOrganisation));

        result.Should().BeTrue();

        _notifyClient.Verify(n => n.SendEmail(It.Is<EmailNotificationRequest>(req =>
            req.EmailAddress == "admin@example.com" &&
            req.TemplateId == "template-id" &&
            req.Personalisation != null &&
            req.Personalisation["org_name"] == "Updated Name" &&
            req.Personalisation["request_link"].Contains(organisation.Guid.ToString())
        )), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldSendEmail_WhenRejectedBuyerChangesEmail()
    {
        var organisation = GivenOrganisation(pendingRoles: [PartyRole.Buyer], reviewedBy: GivenPerson(), reviewComment: "Terrible");

        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.OrganisationEmail,
            Organisation = new OrganisationInfo
            {
                ContactPoint = new OrganisationContactPoint { Email = "foo@bar.com" }
            }
        };

        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(It.IsAny<Guid>())).ReturnsAsync(organisation);
        _organisationRepositoryMock.Setup(repo => repo.FindIncludingReviewedBy(It.IsAny<Guid>())).ReturnsAsync(organisation);

        var result = await _useCase.Execute((Guid.NewGuid(), updateOrganisation));

        result.Should().BeTrue();

        _notifyClient.Verify(n => n.SendEmail(It.Is<EmailNotificationRequest>(req =>
            req.EmailAddress == "admin@example.com" &&
            req.TemplateId == "template-id" &&
            req.Personalisation != null &&
            req.Personalisation["org_name"] == "Acme Ltd" &&
            req.Personalisation["request_link"].Contains(organisation.Guid.ToString())
        )), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldNotSendEmail_WhenRejectedBuyerChangesSomethingOtherThanNameOrEmail()
    {
        var organisation = GivenOrganisation(pendingRoles: [PartyRole.Buyer], reviewedBy: GivenPerson(), reviewComment: "Terrible");

        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.RegisteredAddress,
            Organisation = new OrganisationInfo
            {
                Addresses = [new OrganisationAddress { Country = "UK", CountryName = "UK", Locality = "Devon", PostalCode = "PL1 1LP", StreetAddress = "1 streety street", Type = AddressType.Registered }]
            }
        };

        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(It.IsAny<Guid>())).ReturnsAsync(organisation);

        var result = await _useCase.Execute((Guid.NewGuid(), updateOrganisation));

        result.Should().BeTrue();

        _notifyClient.Verify(n => n.SendEmail(It.IsAny<EmailNotificationRequest>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ShouldNotSendEmail_WhenApprovedBuyerChangesName()
    {
        var organisation = GivenOrganisation();

        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.OrganisationName,
            Organisation = new OrganisationInfo
            {
                OrganisationName = "Updated Name"
            }
        };

        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(It.IsAny<Guid>())).ReturnsAsync(organisation);

        var result = await _useCase.Execute((Guid.NewGuid(), updateOrganisation));

        result.Should().BeTrue();

        _notifyClient.Verify(n => n.SendEmail(It.IsAny<EmailNotificationRequest>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ShouldNotSendEmail_WhenApprovedBuyerChangesEmail()
    {
        var organisation = GivenOrganisation();

        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.OrganisationEmail,
            Organisation = new OrganisationInfo
            {
                ContactPoint = new OrganisationContactPoint { Email = "foo@bar.com" }
            }
        };

        _organisationRepositoryMock.Setup(repo => repo.FindIncludingTenant(It.IsAny<Guid>())).ReturnsAsync(organisation);

        var result = await _useCase.Execute((Guid.NewGuid(), updateOrganisation));

        result.Should().BeTrue();

        _notifyClient.Verify(n => n.SendEmail(It.IsAny<EmailNotificationRequest>()), Times.Never);
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

    private Persistence.Organisation GivenOrganisation(List<PartyRole>? roles = null, List<PartyRole>? pendingRoles = null, Persistence.Person? reviewedBy = null, string? reviewComment = null)
    {
        return new()
        {
            Guid = _organisationId,
            Name = "Acme Ltd",
            Tenant = new Persistence.Tenant()
            {
                Guid = Guid.NewGuid(),
                Name = "Test1"
            },
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
            Roles = roles ?? [],
            PendingRoles = pendingRoles ?? [],
            ReviewedBy = reviewedBy ?? null,
            ReviewComment = reviewComment ?? null
        };
    }

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

    private static Persistence.Person GivenPerson()
    {
        return new Persistence.Person { Guid = new Guid(), FirstName = "First", LastName = "Last", Email = "asd@asd.com" };
    }
}