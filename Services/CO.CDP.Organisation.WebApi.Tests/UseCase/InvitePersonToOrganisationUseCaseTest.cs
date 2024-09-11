using CO.CDP.GovUKNotify;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
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

        _mockGovUKNotifyApiClient.Setup(client => client.SendEmail(It.IsAny<EmailNotificationRequest>()));

        var result = await _useCase.Execute(command);

        result.Should().NotBeNull();
        result.As<PersonInvite>().FirstName.Should().Be("John");
        result.As<PersonInvite>().LastName.Should().Be("Doe");
        result.As<PersonInvite>().Email.Should().Be("john.doe@example.com");
        result.As<PersonInvite>().Guid.Should().Be(_generatedGuid);

        _organisationRepository.Verify(repo => repo.Find(organisationId), Times.Once);
        _personsInviteRepository.Verify(repo => repo.Save(It.IsAny<PersonInvite>()), Times.Once);
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