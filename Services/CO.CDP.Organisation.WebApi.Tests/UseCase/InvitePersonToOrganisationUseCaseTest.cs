using CO.CDP.GovUKNotify;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using GovukNotify.Models;
using Microsoft.Extensions.Configuration;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class InvitePersonToOrganisationUseCaseTest
{
    private readonly Mock<Persistence.IOrganisationRepository> _organisationRepository = new();
    private readonly Mock<Persistence.IPersonInviteRepository> _personsInviteRepository = new();
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
        var inMemorySettings = new Dictionary<string, string> {
            {"GOVUKNotify:PersonInviteEmailTemplateId", "test-template-id"}
        };

        _mockConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task Execute_ValidOrganisationAndInvite_SuccessfullySendsInvite()
    {
        var organisationId = Guid.NewGuid();
        InvitePersonToOrganisation invitePersonData = CreateDummyInviteToPerson();

        var organisation = new Persistence.Organisation
        {
            Guid = organisationId,
            Name = "Test Organisation",
            Tenant = It.IsAny<Tenant>()
        };

        var command = (organisationId, invitePersonData);

        _organisationRepository.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(organisation);

        _mockGovUKNotifyApiClient.Setup(client => client.SendEmail(It.IsAny<EmailNotificationResquest>()));

        var result = await _useCase.Execute(command);

        Assert.NotNull(result);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("john.doe@example.com", result.Email);
        Assert.Equal(_generatedGuid, result.Guid);

        _organisationRepository.Verify(repo => repo.Find(organisationId), Times.Once);
        _personsInviteRepository.Verify(repo => repo.Save(It.IsAny<PersonInvite>()), Times.Once);
        _mockGovUKNotifyApiClient.Verify(client => client.SendEmail(It.IsAny<EmailNotificationResquest>()), Times.Once);               
    }

    [Fact]
    public async Task Execute_UnknownOrganisation_ThrowsUnknownOrganisationException()
    {
        var organisationId = Guid.NewGuid();
        var invitePersonData = CreateDummyInviteToPerson();
        var command = (organisationId, invitePersonData);

        _organisationRepository.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync((Persistence.Organisation?)null);

        await Assert.ThrowsAsync<UnknownOrganisationException>(() => _useCase.Execute(command));
    }

    private InvitePersonToOrganisation CreateDummyInviteToPerson()
    {
        return new InvitePersonToOrganisation
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Scopes = new List<string> { "scope1", "scope2" }
        };
    }
}