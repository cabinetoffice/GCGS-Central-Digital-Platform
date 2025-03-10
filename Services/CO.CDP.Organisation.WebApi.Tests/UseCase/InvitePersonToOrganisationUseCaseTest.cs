using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class InvitePersonToOrganisationUseCaseTest
{
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();
    private readonly Mock<IPersonInviteRepository> _personsInviteRepository = new();
    private readonly Mock<IGovUKNotifyApiClient> _mockGovUKNotifyApiClient = new();
    private readonly IConfiguration _mockConfiguration;
    private readonly Guid _generatedGuid = Guid.NewGuid();

    private InvitePersonToOrganisationUseCase _useCase => new(
        _organisationRepository.Object,
        _personsInviteRepository.Object,
        _mockGovUKNotifyApiClient.Object,
        _mockConfiguration,
        () => _generatedGuid);

    public InvitePersonToOrganisationUseCaseTest()
    {
        var inMemorySettings = new List<KeyValuePair<string, string?>>
        {
            new("GOVUKNotify:PersonInviteEmailTemplateId", "test-template-id"),
            new("OrganisationAppUrl", "http://baseurl/"),
        };

        _mockConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task Execute_ValidInviteForOrganisationWithoutExistingEmail_SuccessfullySendsInvite()
    {
        var organisationId = Guid.NewGuid();
        InvitePersonToOrganisation invitePersonData = CreateDummyInviteToPerson();

        var organisation = new Persistence.Organisation
        {
            Guid = organisationId,
            Name = "Test Organisation",
            Type = OrganisationInformation.OrganisationType.Organisation,
            Tenant = It.IsAny<Tenant>()
        };

        var command = (organisationId, invitePersonData);

        _organisationRepository.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(organisation);

        _organisationRepository.Setup(repo => repo.IsEmailUniqueWithinOrganisation(organisationId, invitePersonData.Email))
            .ReturnsAsync(true);

        _personsInviteRepository.Setup(repo => repo.IsInviteEmailUniqueWithinOrganisation(organisationId, invitePersonData.Email))
            .ReturnsAsync(true);

        _mockGovUKNotifyApiClient.Setup(client => client.SendEmail(It.IsAny<EmailNotificationRequest>()));

        var result = await _useCase.Execute(command);

        result.Should().BeTrue();

        _organisationRepository.Verify(repo => repo.Find(organisationId), Times.Once);
        _organisationRepository.Verify(repo => repo.IsEmailUniqueWithinOrganisation(organisationId, invitePersonData.Email), Times.Once);
        _personsInviteRepository.Verify(repo => repo.SaveNewInvite(It.IsAny<PersonInvite>(), It.IsAny<IEnumerable<PersonInvite>>()), Times.Once);
        _mockGovUKNotifyApiClient.Verify(client => client.SendEmail(It.IsAny<EmailNotificationRequest>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ValidInviteForOrganisationWithExistingEmail_ThrowsException()
    {
        var organisationId = Guid.NewGuid();
        InvitePersonToOrganisation invitePersonData = CreateDummyInviteToPerson();

        var organisation = new Persistence.Organisation
        {
            Guid = organisationId,
            Name = "Test Organisation",
            Type = OrganisationInformation.OrganisationType.Organisation,
            Tenant = It.IsAny<Tenant>()
        };

        var command = (organisationId, invitePersonData);

        _organisationRepository.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(organisation);

        _organisationRepository.Setup(repo => repo.IsEmailUniqueWithinOrganisation(organisationId, invitePersonData.Email))
            .ReturnsAsync(false);

        // Irrelevant as would throw exception before it executes
        _personsInviteRepository.Setup(repo => repo.IsInviteEmailUniqueWithinOrganisation(organisationId, invitePersonData.Email))
            .ReturnsAsync(false);

        _mockGovUKNotifyApiClient.Setup(client => client.SendEmail(It.IsAny<EmailNotificationRequest>()));

        var result = async () => await _useCase.Execute(command);

        await result.Should().ThrowAsync<DuplicateEmailWithinOrganisationException>();

        _organisationRepository.Verify(repo => repo.Find(organisationId), Times.Once);
        _organisationRepository.Verify(repo => repo.IsEmailUniqueWithinOrganisation(organisationId, invitePersonData.Email), Times.Once);
        _personsInviteRepository.Verify(repo => repo.IsInviteEmailUniqueWithinOrganisation(organisationId, invitePersonData.Email), Times.Never);
        _personsInviteRepository.Verify(repo => repo.Save(It.IsAny<PersonInvite>()), Times.Never);
        _mockGovUKNotifyApiClient.Verify(client => client.SendEmail(It.IsAny<EmailNotificationRequest>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ValidInviteWithUniqueEmailForOrganisation_SuccessfullySendsInvite()
    {
        var organisationId = Guid.NewGuid();
        InvitePersonToOrganisation invitePersonData = CreateDummyInviteToPerson();

        var organisation = new Persistence.Organisation
        {
            Guid = organisationId,
            Name = "Test Organisation",
            Type = OrganisationInformation.OrganisationType.Organisation,
            Tenant = It.IsAny<Tenant>()
        };

        var command = (organisationId, invitePersonData);

        _organisationRepository.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(organisation);

        _organisationRepository.Setup(repo => repo.IsEmailUniqueWithinOrganisation(organisationId, invitePersonData.Email))
            .ReturnsAsync(true);

        _personsInviteRepository.Setup(repo => repo.IsInviteEmailUniqueWithinOrganisation(organisationId, invitePersonData.Email))
            .ReturnsAsync(true);

        _mockGovUKNotifyApiClient.Setup(client => client.SendEmail(It.IsAny<EmailNotificationRequest>()));

        var result = await _useCase.Execute(command);

        result.Should().BeTrue();
        _organisationRepository.Verify(repo => repo.Find(organisationId), Times.Once);
        _organisationRepository.Verify(repo => repo.IsEmailUniqueWithinOrganisation(organisationId, invitePersonData.Email), Times.Once);
        _personsInviteRepository.Verify(repo => repo.SaveNewInvite(It.IsAny<PersonInvite>(), It.IsAny<IEnumerable<PersonInvite>>()), Times.Once);
        _mockGovUKNotifyApiClient.Verify(client => client.SendEmail(It.IsAny<EmailNotificationRequest>()), Times.Once);
    }

    [Fact]
public async Task Execute_ValidInviteWithSameEmailForOrganisation_ExpiresExistingInviteAndCreatesAnother()
{
    var organisationId = Guid.NewGuid();
    var existingInviteGuid = Guid.NewGuid();
    InvitePersonToOrganisation invitePersonData = CreateDummyInviteToPerson();

    var organisation = new Persistence.Organisation
    {
        Guid = organisationId,
        Name = "Test Organisation",
        Type = OrganisationInformation.OrganisationType.Organisation,
        Tenant = It.IsAny<Tenant>()
    };

    var existingInvite = new PersonInvite
    {
        Id = 0,
        Guid = existingInviteGuid,
        FirstName = invitePersonData.FirstName,
        LastName = invitePersonData.LastName,
        Email = invitePersonData.Email,
        OrganisationId = 0,
        Organisation = organisation,
        Scopes = [],
        ExpiresOn = null // Not yet expired
    };

    var command = (organisationId, invitePersonData);

    _organisationRepository.Setup(repo => repo.Find(organisationId))
        .ReturnsAsync(organisation);

    _organisationRepository.Setup(repo => repo.IsEmailUniqueWithinOrganisation(organisationId, invitePersonData.Email))
        .ReturnsAsync(true);

    _personsInviteRepository.Setup(repo => repo.IsInviteEmailUniqueWithinOrganisation(organisationId, invitePersonData.Email))
        .ReturnsAsync(true);

    _personsInviteRepository.Setup(repo => repo.FindPersonInviteByEmail(organisationId, invitePersonData.Email))
        .ReturnsAsync(new[] { existingInvite });

    _personsInviteRepository.Setup(repo => repo.Save(It.IsAny<PersonInvite>()));

    var result = await _useCase.Execute(command);

    result.Should().BeTrue();

    existingInvite.ExpiresOn.Should().NotBeNull();

    _personsInviteRepository.Verify(repo => repo.SaveNewInvite(
            It.Is<PersonInvite>(pi => pi.Email == invitePersonData.Email && pi.Guid != existingInviteGuid),
            It.IsAny<IEnumerable<PersonInvite>>()),
        Times.Once);
    _mockGovUKNotifyApiClient.Verify(client => client.SendEmail(It.IsAny<EmailNotificationRequest>()), Times.Once);
}


    [Fact]
    public async Task Execute_UnknownOrganisation_ThrowsUnknownOrganisationException()
    {
        var organisationId = Guid.NewGuid();
        var invitePersonData = CreateDummyInviteToPerson();
        var command = (organisationId, invitePersonData);

        _organisationRepository.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync((Persistence.Organisation?)null);

        Func<Task> act = async () => await _useCase.Execute(command);
        await act.Should().ThrowAsync<UnknownOrganisationException>();
    }

    private InvitePersonToOrganisation CreateDummyInviteToPerson()
    {
        return new InvitePersonToOrganisation
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Scopes = ["scope1", "scope2"]
        };
    }
}